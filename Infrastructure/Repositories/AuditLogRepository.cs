using Infrastructure.Database.DataAccess;
using Infrastructure.Entities;
using Infrastructure.Repositories.Queries;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Repositories;

public sealed class AuditLogRepository
{
    private readonly ISqlDataAccess _dataAccess;

    public AuditLogRepository(ISqlDataAccess dataAccess)
    {
        _dataAccess = dataAccess;
    }

    public async Task<int> CountFilteredAsync(
        AuditLogListFilter filter,
        CancellationToken cancellationToken = default)
    {
        var count = await _dataAccess.ExecuteScalarAsync<int>(
            AuditLogQueries.CountFiltered,
            BuildFilterParameters(filter),
            cancellationToken);
        return count;
    }

    public Task<List<AuditLogs>> GetPagedAsync(
        AuditLogListFilter filter,
        int offset,
        int pageSize,
        string orderByClause,
        CancellationToken cancellationToken = default)
    {
        var sql = $"""
            {AuditLogQueries.GetPagedPrefix}
            ORDER BY {orderByClause}
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
            """;

        var parameters = BuildFilterParameters(filter);
        parameters.Add(new SqlParameter("@Offset", offset));
        parameters.Add(new SqlParameter("@PageSize", pageSize));

        return _dataAccess.QueryAsync<AuditLogs>(sql, parameters, cancellationToken);
    }

    public Task<AuditLogs?> GetByIdAsync(long auditId, CancellationToken cancellationToken = default)
    {
        var parameters = new List<SqlParameter> { new("@AuditId", auditId) };
        return _dataAccess.QueryFirstOrDefaultAsync<AuditLogs>(
            AuditLogQueries.GetById,
            parameters,
            cancellationToken);
    }

    public async Task<IReadOnlyList<string>> GetDistinctCategoriesAsync(
        CancellationToken cancellationToken = default)
        => await ReadStringColumnAsync(AuditLogQueries.DistinctCategories, cancellationToken);

    public async Task<IReadOnlyList<string>> GetDistinctEntityTypesAsync(
        CancellationToken cancellationToken = default)
        => await ReadStringColumnAsync(AuditLogQueries.DistinctEntityTypes, cancellationToken);

    public async Task<IReadOnlyList<string>> GetDistinctUserNamesAsync(
        CancellationToken cancellationToken = default)
        => await ReadStringColumnAsync(AuditLogQueries.DistinctUserNames, cancellationToken);

    private async Task<IReadOnlyList<string>> ReadStringColumnAsync(
        string sql,
        CancellationToken cancellationToken)
    {
        var rows = await _dataAccess.QueryAsync<AuditLogStringValue>(sql, null, cancellationToken);
        return rows
            .Select(row => row.Value)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!)
            .ToList();
    }

    private static List<SqlParameter> BuildFilterParameters(AuditLogListFilter filter)
    {
        var search = NormalizeOptional(filter.Search);
        var userName = NormalizeOptional(filter.UserName);
        var category = NormalizeOptional(filter.Category);
        var outcome = NormalizeOptional(filter.Outcome);
        var entityType = NormalizeOptional(filter.EntityType);
        var clientIp = NormalizeOptional(filter.ClientIp);

        DateTime? dateFrom = filter.DateFrom?.Date;
        DateTime? dateToExclusive = filter.DateTo.HasValue
            ? filter.DateTo.Value.Date.AddDays(1)
            : null;

        return
        [
            new SqlParameter("@Search", (object?)search ?? DBNull.Value),
            new SqlParameter(
                "@SearchPattern",
                search is null ? DBNull.Value : $"%{EscapeLike(search)}%"),
            new SqlParameter("@DateFrom", (object?)dateFrom ?? DBNull.Value),
            new SqlParameter("@DateToExclusive", (object?)dateToExclusive ?? DBNull.Value),
            new SqlParameter("@UserName", (object?)userName ?? DBNull.Value),
            new SqlParameter("@Category", (object?)category ?? DBNull.Value),
            new SqlParameter("@Outcome", (object?)outcome ?? DBNull.Value),
            new SqlParameter("@EntityType", (object?)entityType ?? DBNull.Value),
            new SqlParameter("@ClientIp", (object?)clientIp ?? DBNull.Value),
            new SqlParameter(
                "@ClientIpPattern",
                clientIp is null ? DBNull.Value : $"%{EscapeLike(clientIp)}%"),
        ];
    }

    private static string? NormalizeOptional(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
    }

    private static string EscapeLike(string value) =>
        value.Replace("[", "[[]", StringComparison.Ordinal)
            .Replace("%", "[%]", StringComparison.Ordinal)
            .Replace("_", "[_]", StringComparison.Ordinal);

    /// <summary>Maps single-column DISTINCT queries (`value` alias).</summary>
    private sealed class AuditLogStringValue
    {
        [Infrastructure.Data.DbManager.DbColumn("value")]
        public string? Value { get; set; }
    }
}

public sealed class AuditLogListFilter
{
    public string? Search { get; init; }
    public DateTime? DateFrom { get; init; }
    public DateTime? DateTo { get; init; }
    public string? UserName { get; init; }
    public string? Category { get; init; }
    public string? Outcome { get; init; }
    public string? EntityType { get; init; }
    public string? ClientIp { get; init; }
}
