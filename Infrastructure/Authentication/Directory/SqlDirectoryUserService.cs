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
/// SQL dizin. Öncelik sırası:
/// 1) <c>user_roles</c> + <c>user_company_access</c> (canonical: rol ⊥ şirket)
/// 2) <c>user_company_roles</c> (legacy coupled)
/// 3) <c>users.role_id</c> fallback
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
            var separated = await TryQuerySeparatedAccessAsync(
                identity,
                cancellationToken);

            if (separated is not null)
            {
                return separated;
            }

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

    private async Task<ApplicationUser?> TryQuerySeparatedAccessAsync(
        ExternalIdentity identity,
        CancellationToken cancellationToken)
    {
        const string userSql = """
            SELECT
                u.user_id           AS UserId,
                u.is_active         AS IsActive,
                u.email             AS Email,
                u.full_name         AS FullName,
                u.username          AS Username
            FROM users u
            WHERE u.azure_object_id = @ExternalId;
            """;

        List<UserHeaderRow> headers;
        try
        {
            headers = await QueryAsync<UserHeaderRow>(userSql, identity.ExternalId, cancellationToken);
        }
        catch (SqlException)
        {
            return null;
        }

        if (headers.Count == 0)
        {
            return null;
        }

        var header = headers[0];

        List<RolePermissionRow> roleRows;
        List<CompanyAccessRow> companyRows;
        try
        {
            roleRows = await QueryByUserIdAsync<RolePermissionRow>(
                """
                SELECT
                    r.role_id AS RoleId,
                    r.role_name AS RoleName,
                    p.permission_code AS PermissionCode
                FROM user_roles ur
                INNER JOIN roles r ON r.role_id = ur.role_id AND r.is_active = 1
                LEFT JOIN role_permissions rp ON rp.role_id = ur.role_id
                LEFT JOIN permissions p ON p.permission_id = rp.permission_id
                WHERE ur.user_id = @UserId
                  AND ur.is_active = 1;
                """,
                header.UserId,
                cancellationToken);

            companyRows = await QueryByUserIdAsync<CompanyAccessRow>(
                """
                SELECT
                    uca.company_id AS CompanyId,
                    c.company_name AS CompanyName,
                    c.company_code AS CompanyCode
                FROM user_company_access uca
                INNER JOIN companies c ON c.company_id = uca.company_id AND c.is_active = 1
                WHERE uca.user_id = @UserId
                  AND uca.is_active = 1;
                """,
                header.UserId,
                cancellationToken);
        }
        catch (SqlException ex)
        {
            _logger.LogDebug(
                ex,
                "user_roles/user_company_access henüz yok; legacy yola düşülüyor.");
            return null;
        }

        // Yeni tablolar boşsa legacy'ye düş (henüz backfill olmamış kullanıcılar).
        if (roleRows.Count == 0 && companyRows.Count == 0)
        {
            return null;
        }

        return MapFromSeparated(identity, header, roleRows, companyRows);
    }

    private async Task<List<MembershipRow>?> TryQueryMembershipsAsync(
        string externalId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                u.user_id           AS UserId,
                u.is_active         AS IsActive,
                u.email             AS Email,
                u.full_name         AS FullName,
                u.username          AS Username,
                ucr.company_id      AS CompanyId,
                c.company_name      AS CompanyName,
                r.role_name         AS RoleName,
                p.permission_code   AS PermissionCode
            FROM users u
            INNER JOIN user_company_roles ucr
                ON ucr.user_id = u.user_id AND ucr.is_active = 1
            LEFT JOIN companies c ON c.company_id = ucr.company_id
            LEFT JOIN roles r ON r.role_id = ucr.role_id
            LEFT JOIN role_permissions rp ON rp.role_id = ucr.role_id
            LEFT JOIN permissions p ON p.permission_id = rp.permission_id
            WHERE u.azure_object_id = @ExternalId;
            """;

        try
        {
            return await QueryAsync<MembershipRow>(sql, externalId, cancellationToken);
        }
        catch (SqlException ex)
        {
            _logger.LogDebug(
                ex,
                "user_company_roles join başarısız; role_id yedek yoluna düşülüyor.");
            return null;
        }
    }

    private async Task<List<FallbackUserRow>?> TryQueryUserWithRoleAsync(
        string externalId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                u.user_id           AS UserId,
                u.is_active         AS IsActive,
                u.email             AS Email,
                u.full_name         AS FullName,
                u.username          AS Username,
                u.role_id           AS RoleId,
                r.role_name         AS RoleName,
                u.branch_id         AS BranchId,
                p.permission_code   AS PermissionCode
            FROM users u
            LEFT JOIN roles r ON r.role_id = u.role_id
            LEFT JOIN role_permissions rp ON rp.role_id = u.role_id
            LEFT JOIN permissions p ON p.permission_id = rp.permission_id
            WHERE u.azure_object_id = @ExternalId;
            """;

        var rows = await QueryAsync<FallbackUserRow>(sql, externalId, cancellationToken);
        return rows.Count == 0 ? null : rows;
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

        return await QueryCoreAsync<T>(sql, parameters, cancellationToken);
    }

    private async Task<List<T>> QueryByUserIdAsync<T>(
        string sql,
        int userId,
        CancellationToken cancellationToken)
        where T : class, new()
    {
        SqlParameter[] parameters =
        [
            new SqlParameter("@UserId", SqlDbType.Int) { Value = userId }
        ];

        return await QueryCoreAsync<T>(sql, parameters, cancellationToken);
    }

    private async Task<List<T>> QueryCoreAsync<T>(
        string sql,
        IEnumerable<SqlParameter> parameters,
        CancellationToken cancellationToken)
        where T : class, new()
    {
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

    private static ApplicationUser MapFromSeparated(
        ExternalIdentity identity,
        UserHeaderRow header,
        List<RolePermissionRow> roleRows,
        List<CompanyAccessRow> companyRows)
    {
        var roles = roleRows
            .Select(r => r.RoleName)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var dbPermissions = roleRows
            .Select(r => r.PermissionCode)
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Select(code => code!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var permissions = ResolvePermissions(dbPermissions, roles);
        var hasGlobal = roles.Contains(Roles.GlobalAdmin, StringComparer.OrdinalIgnoreCase);

        var memberships = companyRows
            .Select(row => new CompanyMembership
            {
                CompanyId = row.CompanyId,
                CompanyName = row.CompanyName,
                Roles = roles,
                Permissions = permissions
            })
            .ToArray();

        var primary = memberships.FirstOrDefault();

        return new ApplicationUser
        {
            Identity = EnrichIdentity(identity, header.Email, header.FullName, header.Username),
            UserId = header.UserId,
            IsActive = header.IsActive,
            CompanyId = primary?.CompanyId,
            CompanyName = primary?.CompanyName,
            Roles = roles,
            Permissions = permissions,
            CompanyMemberships = memberships,
            HasGlobalOrganizationAccess = hasGlobal
        };
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

                var dbPermissions = group
                    .Select(r => r.PermissionCode)
                    .Where(code => !string.IsNullOrWhiteSpace(code))
                    .Select(code => code!)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray();

                return new CompanyMembership
                {
                    CompanyId = group.Key,
                    CompanyName = group.Select(r => r.CompanyName).FirstOrDefault(n => n is not null),
                    Roles = roles,
                    Permissions = ResolvePermissions(dbPermissions, roles)
                };
            })
            .ToArray();

        var allRoles = memberships
            .SelectMany(m => m.Roles)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var allDbPermissions = rows
            .Select(r => r.PermissionCode)
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Select(code => code!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var primary = memberships.FirstOrDefault();
        var hasGlobal = allRoles.Contains(Roles.GlobalAdmin, StringComparer.OrdinalIgnoreCase);

        return new ApplicationUser
        {
            Identity = EnrichIdentity(identity, first.Email, first.FullName, first.Username),
            UserId = first.UserId,
            IsActive = first.IsActive,
            CompanyId = primary?.CompanyId,
            CompanyName = primary?.CompanyName,
            Roles = allRoles,
            Permissions = ResolvePermissions(allDbPermissions, allRoles),
            CompanyMemberships = memberships,
            HasGlobalOrganizationAccess = hasGlobal
        };
    }

    private static ApplicationUser MapFromFallback(
        ExternalIdentity identity,
        List<FallbackUserRow> rows)
    {
        var first = rows[0];
        var roles = string.IsNullOrWhiteSpace(first.RoleName)
            ? Array.Empty<string>()
            : new[] { first.RoleName };

        var dbPermissions = rows
            .Select(r => r.PermissionCode)
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Select(code => code!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var permissions = ResolvePermissions(dbPermissions, roles);
        var hasGlobal = roles.Contains(Roles.GlobalAdmin, StringComparer.OrdinalIgnoreCase);

        return new ApplicationUser
        {
            Identity = EnrichIdentity(identity, first.Email, first.FullName, first.Username),
            UserId = first.UserId,
            IsActive = first.IsActive,
            CompanyId = null,
            CompanyName = null,
            Roles = roles,
            Permissions = permissions,
            CompanyMemberships = [],
            HasGlobalOrganizationAccess = hasGlobal
        };
    }

    private static IReadOnlyCollection<string> ResolvePermissions(
        IReadOnlyCollection<string> dbPermissions,
        IReadOnlyCollection<string> roles)
    {
        if (dbPermissions.Count > 0)
        {
            return dbPermissions;
        }

        return DirectoryPermissionDefaults.Apply(roles, []);
    }

    private static ExternalIdentity EnrichIdentity(
        ExternalIdentity identity,
        string? email,
        string? fullName,
        string? username) =>
        new()
        {
            IdentityProvider = identity.IdentityProvider,
            ExternalId = identity.ExternalId,
            DomainOrTenant = identity.DomainOrTenant,
            Username = identity.Username ?? username,
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

    private sealed class UserHeaderRow
    {
        public int UserId { get; set; }

        public bool IsActive { get; set; }

        public string? Email { get; set; }

        public string? FullName { get; set; }

        public string? Username { get; set; }
    }

    private sealed class RolePermissionRow
    {
        public int RoleId { get; set; }

        public string? RoleName { get; set; }

        public string? PermissionCode { get; set; }
    }

    private sealed class CompanyAccessRow
    {
        public int CompanyId { get; set; }

        public string? CompanyName { get; set; }

        public string? CompanyCode { get; set; }
    }

    private sealed class MembershipRow
    {
        public int UserId { get; set; }

        public bool IsActive { get; set; }

        public string? Email { get; set; }

        public string? FullName { get; set; }

        public string? Username { get; set; }

        public int? CompanyId { get; set; }

        public string? CompanyName { get; set; }

        public string? RoleName { get; set; }

        public string? PermissionCode { get; set; }
    }

    private sealed class FallbackUserRow
    {
        public int UserId { get; set; }

        public bool IsActive { get; set; }

        public string? Email { get; set; }

        public string? FullName { get; set; }

        public string? Username { get; set; }

        public int RoleId { get; set; }

        public string? RoleName { get; set; }

        public int? BranchId { get; set; }

        public string? PermissionCode { get; set; }
    }
}
