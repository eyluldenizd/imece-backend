using System.Data;
using Core.Authentication;
using Core.Authorization;
using Core.Directory;
using Infrastructure.Database.Connections;
using Infrastructure.Database.DataAccess;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Authentication.Directory;

/// <summary>
/// SQL tabanlı dizin. Kullanıcıyı <c>azure_object_id</c> ile bulur; varsa
/// <c>user_company_roles</c> üzerinden çoklu şirket üyeliklerini, yoksa
/// <c>users.role_id</c> + <c>roles</c> yedek yolunu kullanır. SQL hatalarında
/// veya kayıt yoksa <c>null</c> döner (kayıtsız kullanıcıya düşülür).
/// </summary>
public sealed class SqlDirectoryUserService : IDirectoryUserService
{
    private readonly ISqlDataAccess? _sql;
    private readonly IDbConnectionFactory? _connectionFactory;
    private readonly ILogger<SqlDirectoryUserService> _logger;

    public SqlDirectoryUserService(
        IServiceProvider services,
        ILogger<SqlDirectoryUserService> logger)
    {
        _sql = services.GetService<ISqlDataAccess>();
        _connectionFactory = services.GetService<IDbConnectionFactory>();
        _logger = logger;
    }

    public async Task<ApplicationUser?> FindByExternalIdentityAsync(
        ExternalIdentity identity,
        CancellationToken cancellationToken = default)
    {
        if (_sql is null && _connectionFactory is null)
        {
            _logger.LogWarning(
                "SqlDirectoryUserService için ISqlDataAccess veya IDbConnectionFactory kayıtlı değil.");
            return null;
        }

        try
        {
            var membershipRows = await TryQueryMembershipsAsync(
                identity.ExternalId,
                cancellationToken);

            if (membershipRows is { Count: > 0 })
            {
                return MapFromMembershipRows(identity, membershipRows);
            }

            var fallback = await TryQueryUserWithRoleAsync(
                identity.ExternalId,
                cancellationToken);

            return fallback is null
                ? null
                : MapFromFallback(identity, fallback);
        }
        catch (Exception ex) when (ex is SqlException or InvalidOperationException)
        {
            _logger.LogWarning(
                ex,
                "SQL dizin sorgusu başarısız (ExternalId={ExternalId}).",
                identity.ExternalId);
            return null;
        }
    }

    private async Task<List<MembershipRow>?> TryQueryMembershipsAsync(
        string externalId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                u.user_id       AS UserId,
                u.is_active     AS IsActive,
                u.email         AS Email,
                u.full_name     AS FullName,
                ucr.company_id  AS CompanyId,
                c.company_name  AS CompanyName,
                r.role_name     AS RoleName
            FROM users u
            INNER JOIN user_company_roles ucr ON ucr.user_id = u.user_id
            LEFT JOIN companies c ON c.company_id = ucr.company_id
            LEFT JOIN roles r ON r.role_id = ucr.role_id
            WHERE u.azure_object_id = @ExternalId;
            """;

        try
        {
            return await QueryAsync<MembershipRow>(sql, externalId, cancellationToken);
        }
        catch (SqlException ex)
        {
            // Tablo yok / şema uyumsuz → yedek yola düş.
            _logger.LogDebug(
                ex,
                "user_company_roles join başarısız; role_id yedek yoluna düşülüyor.");
            return null;
        }
    }

    private async Task<FallbackUserRow?> TryQueryUserWithRoleAsync(
        string externalId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                u.user_id   AS UserId,
                u.is_active AS IsActive,
                u.email     AS Email,
                u.full_name AS FullName,
                u.role_id   AS RoleId,
                r.role_name AS RoleName,
                u.branch_id AS BranchId
            FROM users u
            LEFT JOIN roles r ON r.role_id = u.role_id
            WHERE u.azure_object_id = @ExternalId;
            """;

        var rows = await QueryAsync<FallbackUserRow>(sql, externalId, cancellationToken);
        return rows.FirstOrDefault();
    }

    private async Task<List<T>> QueryAsync<T>(
        string sql,
        string externalId,
        CancellationToken cancellationToken)
        where T : class, new()
    {
        SqlParameter[] parameters =
        [
            new SqlParameter("@ExternalId", SqlDbType.NVarChar, 255)
            {
                Value = externalId
            }
        ];

        if (_sql is not null)
        {
            return await _sql.QueryAsync<T>(sql, parameters, cancellationToken);
        }

        return await QueryWithConnectionFactoryAsync<T>(sql, parameters, cancellationToken);
    }

    private async Task<List<T>> QueryWithConnectionFactoryAsync<T>(
        string sql,
        IEnumerable<SqlParameter> parameters,
        CancellationToken cancellationToken)
        where T : class, new()
    {
        if (_connectionFactory is null)
        {
            return [];
        }

        await using var connection =
            await _connectionFactory.OpenApplicationConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = sql;

        foreach (var parameter in parameters)
        {
            command.Parameters.Add(parameter);
        }

        var results = new List<T>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var ordinals = BuildOrdinalMap(reader);

        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(MapReaderToObject<T>(reader, ordinals));
        }

        return results;
    }

    private static ApplicationUser MapFromMembershipRows(
        ExternalIdentity identity,
        List<MembershipRow> rows)
    {
        var first = rows[0];
        var memberships = rows
            .Where(row => row.CompanyId.HasValue)
            .GroupBy(row => row.CompanyId!.Value)
            .Select(group =>
            {
                var roles = group
                    .Select(r => r.RoleName)
                    .Where(name => !string.IsNullOrWhiteSpace(name))
                    .Select(name => name!)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray();

                return new CompanyMembership
                {
                    CompanyId = group.Key,
                    CompanyName = group.Select(r => r.CompanyName).FirstOrDefault(n => n is not null),
                    Roles = roles,
                    Permissions = DirectoryPermissionDefaults.Apply(roles, [])
                };
            })
            .ToArray();

        var allRoles = memberships
            .SelectMany(m => m.Roles)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var primary = memberships.FirstOrDefault();

        return new ApplicationUser
        {
            Identity = EnrichIdentity(identity, first.Email, first.FullName),
            UserId = first.UserId,
            IsActive = first.IsActive,
            CompanyId = primary?.CompanyId,
            CompanyName = primary?.CompanyName,
            Roles = allRoles,
            Permissions = DirectoryPermissionDefaults.Apply(allRoles, []),
            CompanyMemberships = memberships
        };
    }

    private static ApplicationUser MapFromFallback(
        ExternalIdentity identity,
        FallbackUserRow row)
    {
        var roles = string.IsNullOrWhiteSpace(row.RoleName)
            ? Array.Empty<string>()
            : new[] { row.RoleName };

        IReadOnlyCollection<CompanyMembership> memberships = [];
        int? companyId = null;
        string? companyName = null;

        // users üzerinde company alanı yoksa branch_id ile pragmatik üyelik.
        if (row.BranchId is int branchId)
        {
            companyId = branchId;
            companyName = null;
            memberships =
            [
                new CompanyMembership
                {
                    CompanyId = branchId,
                    CompanyName = null,
                    Roles = roles,
                    Permissions = DirectoryPermissionDefaults.Apply(roles, [])
                }
            ];
        }

        return new ApplicationUser
        {
            Identity = EnrichIdentity(identity, row.Email, row.FullName),
            UserId = row.UserId,
            IsActive = row.IsActive,
            CompanyId = companyId,
            CompanyName = companyName,
            Roles = roles,
            Permissions = DirectoryPermissionDefaults.Apply(roles, []),
            CompanyMemberships = memberships
        };
    }

    private static ExternalIdentity EnrichIdentity(
        ExternalIdentity identity,
        string? email,
        string? fullName) =>
        new()
        {
            IdentityProvider = identity.IdentityProvider,
            ExternalId = identity.ExternalId,
            DomainOrTenant = identity.DomainOrTenant,
            Username = identity.Username,
            Email = identity.Email ?? email,
            DisplayName = identity.DisplayName ?? fullName
        };

    private static Dictionary<string, int> BuildOrdinalMap(SqlDataReader reader)
    {
        var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < reader.FieldCount; i++)
        {
            map[reader.GetName(i)] = i;
        }

        return map;
    }

    private static T MapReaderToObject<T>(
        SqlDataReader reader,
        Dictionary<string, int> ordinals)
        where T : class, new()
    {
        var item = new T();

        foreach (var property in typeof(T).GetProperties())
        {
            if (!property.CanWrite)
            {
                continue;
            }

            if (!ordinals.TryGetValue(property.Name, out var ordinal) || reader.IsDBNull(ordinal))
            {
                continue;
            }

            var value = reader.GetValue(ordinal);
            var targetType = Nullable.GetUnderlyingType(property.PropertyType)
                ?? property.PropertyType;
            property.SetValue(item, Convert.ChangeType(value, targetType));
        }

        return item;
    }

    private sealed class MembershipRow
    {
        public int UserId { get; set; }

        public bool IsActive { get; set; }

        public string? Email { get; set; }

        public string? FullName { get; set; }

        public int? CompanyId { get; set; }

        public string? CompanyName { get; set; }

        public string? RoleName { get; set; }
    }

    private sealed class FallbackUserRow
    {
        public int UserId { get; set; }

        public bool IsActive { get; set; }

        public string? Email { get; set; }

        public string? FullName { get; set; }

        public int RoleId { get; set; }

        public string? RoleName { get; set; }

        public int? BranchId { get; set; }
    }
}
