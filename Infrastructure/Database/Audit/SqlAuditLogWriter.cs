using Infrastructure.Database.Connections;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Database.Audit;

public sealed class SqlAuditLogWriter : IAuditLogWriter
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IDbExecutor _executor;
    private readonly ILogger<SqlAuditLogWriter> _logger;

    public SqlAuditLogWriter(
        IDbConnectionFactory connectionFactory,
        IDbExecutor executor,
        ILogger<SqlAuditLogWriter> logger)
    {
        _connectionFactory = connectionFactory;
        _executor = executor;
        _logger = logger;
    }

    public async Task WriteAsync(
        AuditLogEntry entry,
        CancellationToken cancellationToken = default)
    {
        await using var connection =
            await _connectionFactory.OpenApplicationConnectionAsync(cancellationToken);

        await _executor.ExecuteNonQueryAsync(
            connection,
            """
            INSERT INTO [dbo].[audit_log]
            (
                occurred_at, action, category, outcome,
                entity_type, entity_id,
                user_id, company_id, trace_id, client_ip, user_agent,
                client_application, http_method, request_path, status_code, duration_ms,
                error_code, exception_type,
                before_json, after_json, request_body_json
            )
            VALUES
            (
                SYSUTCDATETIME(), @Action, @Category, @Outcome,
                @EntityType, @EntityId,
                @UserId, @CompanyId, @TraceId, @ClientIp, @UserAgent,
                @ClientApplication, @HttpMethod, @RequestPath, @StatusCode, @DurationMs,
                @ErrorCode, @ExceptionType,
                @BeforeJson, @AfterJson, @RequestBodyJson
            );
            """,
            parameters:
            [
                new SqlParameter("@Action", entry.Action),
                new SqlParameter("@Category", (object?)entry.Category ?? DBNull.Value),
                new SqlParameter("@Outcome", (object?)entry.Outcome ?? DBNull.Value),
                new SqlParameter("@EntityType", (object?)entry.EntityType ?? DBNull.Value),
                new SqlParameter("@EntityId", (object?)entry.EntityId ?? DBNull.Value),
                new SqlParameter("@UserId", (object?)entry.UserId ?? DBNull.Value),
                new SqlParameter("@CompanyId", (object?)entry.CompanyId ?? DBNull.Value),
                new SqlParameter("@TraceId", (object?)entry.TraceId ?? DBNull.Value),
                new SqlParameter("@ClientIp", (object?)entry.ClientIp ?? DBNull.Value),
                new SqlParameter("@UserAgent", (object?)entry.UserAgent ?? DBNull.Value),
                new SqlParameter("@ClientApplication", (object?)entry.ClientApplication ?? DBNull.Value),
                new SqlParameter("@HttpMethod", (object?)entry.HttpMethod ?? DBNull.Value),
                new SqlParameter("@RequestPath", (object?)entry.RequestPath ?? DBNull.Value),
                new SqlParameter("@StatusCode", (object?)entry.StatusCode ?? DBNull.Value),
                new SqlParameter("@DurationMs", (object?)entry.DurationMs ?? DBNull.Value),
                new SqlParameter("@ErrorCode", (object?)entry.ErrorCode ?? DBNull.Value),
                new SqlParameter("@ExceptionType", (object?)entry.ExceptionType ?? DBNull.Value),
                new SqlParameter("@BeforeJson", (object?)entry.BeforeJson ?? DBNull.Value),
                new SqlParameter("@AfterJson", (object?)entry.AfterJson ?? DBNull.Value),
                new SqlParameter("@RequestBodyJson", (object?)entry.RequestBodyJson ?? DBNull.Value)
            ],
            cancellationToken: cancellationToken);

        _logger.LogDebug(
            "Audit kaydı yazıldı: {Category}/{Action} {EntityType}/{EntityId}",
            entry.Category,
            entry.Action,
            entry.EntityType,
            entry.EntityId);
    }
}
