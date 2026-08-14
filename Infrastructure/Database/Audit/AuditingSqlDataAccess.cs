using Core.Auditing;
using Infrastructure.Database.DataAccess;
using Infrastructure.Database.Options;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Database.Audit;

/// <summary>
/// SQL yazma işlemlerini (INSERT/UPDATE/DELETE) otomatik Sql kategorisinde loglar.
/// audit_log yazımlarını atlar (özyineleme yok). SELECT loglanmaz.
/// </summary>
public sealed class AuditingSqlDataAccess : ISqlDataAccess
{
    private readonly ISqlDataAccess _inner;
    private readonly IAuditService _auditService;
    private readonly IOptions<AuditOptions> _options;
    private readonly ILogger<AuditingSqlDataAccess> _logger;

    public AuditingSqlDataAccess(
        ISqlDataAccess inner,
        IAuditService auditService,
        IOptions<AuditOptions> options,
        ILogger<AuditingSqlDataAccess> logger)
    {
        _inner = inner;
        _auditService = auditService;
        _options = options;
        _logger = logger;
    }

    public Task<List<T>> QueryAsync<T>(
        string sql,
        IEnumerable<SqlParameter>? parameters = null,
        CancellationToken cancellationToken = default) where T : new() =>
        _inner.QueryAsync<T>(sql, parameters, cancellationToken);

    public Task<T?> QueryFirstOrDefaultAsync<T>(
        string sql,
        IEnumerable<SqlParameter>? parameters = null,
        CancellationToken cancellationToken = default) where T : new() =>
        _inner.QueryFirstOrDefaultAsync<T>(sql, parameters, cancellationToken);

    public async Task<int> ExecuteAsync(
        string sql,
        IEnumerable<SqlParameter>? parameters = null,
        CancellationToken cancellationToken = default)
    {
        var affected = await _inner.ExecuteAsync(sql, parameters, cancellationToken);
        await TryAuditSqlWriteAsync(sql, affected, cancellationToken);
        return affected;
    }

    public async Task<T?> ExecuteScalarAsync<T>(
        string sql,
        IEnumerable<SqlParameter>? parameters = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _inner.ExecuteScalarAsync<T>(sql, parameters, cancellationToken);
        await TryAuditSqlWriteAsync(sql, result, cancellationToken);
        return result;
    }

    public Task ExecuteInTransactionAsync(
        Func<SqlConnection, SqlTransaction, CancellationToken, Task> work,
        CancellationToken cancellationToken = default) =>
        _inner.ExecuteInTransactionAsync(work, cancellationToken);

    public Task<T> ExecuteInTransactionAsync<T>(
        Func<SqlConnection, SqlTransaction, CancellationToken, Task<T>> work,
        CancellationToken cancellationToken = default) =>
        _inner.ExecuteInTransactionAsync(work, cancellationToken);

    private async Task TryAuditSqlWriteAsync(
        string sql,
        object? result,
        CancellationToken cancellationToken)
    {
        var options = _options.Value;
        if (!options.Enabled || !options.CaptureSqlWrites)
        {
            return;
        }

        if (!IsWriteSql(sql) || IsAuditLogSql(sql))
        {
            return;
        }

        try
        {
            await _auditService.WriteAsync(
                new AuditEvent
                {
                    Action = $"Sql.{ResolveWriteKind(sql)}",
                    Category = AuditCategories.Sql,
                    Outcome = AuditOutcomes.Success,
                    After = new
                    {
                        sqlPreview = TruncateSql(sql),
                        result
                    }
                },
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "SQL audit yazılamadı.");
        }
    }

    private static bool IsWriteSql(string sql)
    {
        var trimmed = sql.AsSpan().TrimStart();
        return StartsWithIgnoreCase(trimmed, "INSERT")
            || StartsWithIgnoreCase(trimmed, "UPDATE")
            || StartsWithIgnoreCase(trimmed, "DELETE")
            || StartsWithIgnoreCase(trimmed, "MERGE");
    }

    private static bool IsAuditLogSql(string sql) =>
        sql.Contains("audit_log", StringComparison.OrdinalIgnoreCase);

    private static string ResolveWriteKind(string sql)
    {
        var trimmed = sql.AsSpan().TrimStart();
        if (StartsWithIgnoreCase(trimmed, "INSERT")) return "Insert";
        if (StartsWithIgnoreCase(trimmed, "UPDATE")) return "Update";
        if (StartsWithIgnoreCase(trimmed, "DELETE")) return "Delete";
        if (StartsWithIgnoreCase(trimmed, "MERGE")) return "Merge";
        return "Write";
    }

    private static bool StartsWithIgnoreCase(ReadOnlySpan<char> value, string prefix) =>
        value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);

    private static string TruncateSql(string sql)
    {
        var compact = string.Join(' ', sql.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        return compact.Length <= 500 ? compact : compact[..500] + "…";
    }
}
