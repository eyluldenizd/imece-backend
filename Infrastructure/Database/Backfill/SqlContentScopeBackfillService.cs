using Infrastructure.Database.Connections;
using Infrastructure.Database.Options;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Database.Backfill;

public sealed class SqlContentScopeBackfillService : IContentScopeBackfillService
{
    private static readonly string[] ScopeTypeTables =
    [
        "media_folders",
        "media_files",
        "announcements",
        "events"
    ];

    private static readonly string[] CompanyScopedTables =
    [
        "media_folders",
        "media_files",
        "announcements",
        "events",
        "weekly_menu_entries",
        "reservations"
    ];

    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IDbExecutor _executor;
    private readonly IOptions<ContentScopeBackfillOptions> _options;
    private readonly ILogger<SqlContentScopeBackfillService> _logger;

    public SqlContentScopeBackfillService(
        IDbConnectionFactory connectionFactory,
        IDbExecutor executor,
        IOptions<ContentScopeBackfillOptions> options,
        ILogger<SqlContentScopeBackfillService> logger)
    {
        _connectionFactory = connectionFactory;
        _executor = executor;
        _options = options;
        _logger = logger;
    }

    public async Task<ContentScopeBackfillReport> RunAsync(
        CancellationToken cancellationToken = default)
    {
        var options = _options.Value;
        await using var connection =
            await _connectionFactory.OpenApplicationConnectionAsync(cancellationToken);

        int? defaultCompanyId = null;
        if (!string.IsNullOrWhiteSpace(options.DefaultCompanyCode))
        {
            var scalar = await _executor.ExecuteScalarAsync(
                connection,
                """
                SELECT TOP 1 company_id
                FROM [dbo].[companies]
                WHERE company_code = @Code;
                """,
                parameters: [new SqlParameter("@Code", options.DefaultCompanyCode)],
                cancellationToken: cancellationToken);

            defaultCompanyId = scalar switch
            {
                int i => i,
                long l => (int)l,
                _ => null
            };
        }

        var updated = 0;
        if (!options.DryRun && defaultCompanyId is not null)
        {
            foreach (var table in CompanyScopedTables)
            {
                if (!await ColumnExistsAsync(connection, table, "company_id", cancellationToken))
                {
                    continue;
                }

                var affected = await _executor.ExecuteNonQueryAsync(
                    connection,
                    $"""
                    ;WITH cte AS (
                        SELECT TOP (@BatchSize) company_id
                        FROM [dbo].[{table}]
                        WHERE company_id IS NULL
                    )
                    UPDATE cte SET company_id = @CompanyId;
                    """,
                    parameters:
                    [
                        new SqlParameter("@BatchSize", options.BatchSize),
                        new SqlParameter("@CompanyId", defaultCompanyId.Value)
                    ],
                    cancellationToken: cancellationToken);

                updated += affected;
            }
        }

        if (!options.DryRun)
        {
            foreach (var table in ScopeTypeTables)
            {
                if (!await ColumnExistsAsync(connection, table, "scope_type", cancellationToken))
                {
                    continue;
                }

                var scopeUpdated = await _executor.ExecuteNonQueryAsync(
                    connection,
                    $"""
                    UPDATE [dbo].[{table}]
                    SET scope_type = CASE
                        WHEN company_id IS NULL THEN N'Global'
                        ELSE N'Company'
                    END
                    WHERE scope_type IS NULL
                       OR (company_id IS NULL AND scope_type <> N'Global')
                       OR (company_id IS NOT NULL AND scope_type <> N'Company');
                    """,
                    cancellationToken: cancellationToken);

                updated += scopeUpdated;
            }
        }

        var remaining = 0;
        foreach (var table in CompanyScopedTables)
        {
            if (!await ColumnExistsAsync(connection, table, "company_id", cancellationToken))
            {
                continue;
            }

            var count = await _executor.ExecuteScalarAsync(
                connection,
                $"SELECT COUNT(1) FROM [dbo].[{table}] WHERE company_id IS NULL;",
                cancellationToken: cancellationToken);

            remaining += count switch
            {
                int i => i,
                long l => (int)l,
                _ => 0
            };
        }

        _logger.LogInformation(
            "Content scope backfill tamamlandı. DryRun={DryRun} Updated={Updated} Remaining={Remaining}",
            options.DryRun, updated, remaining);

        return new ContentScopeBackfillReport
        {
            DryRun = options.DryRun,
            DefaultCompanyId = defaultCompanyId,
            TotalUnassignedAfter = remaining,
            UpdatedRows = updated
        };
    }

    private async Task<bool> ColumnExistsAsync(
        SqlConnection connection,
        string tableName,
        string columnName,
        CancellationToken cancellationToken)
    {
        var result = await _executor.ExecuteScalarAsync(
            connection,
            """
            SELECT CASE WHEN COL_LENGTH(@Table, @Column) IS NULL THEN 0 ELSE 1 END;
            """,
            parameters:
            [
                new SqlParameter("@Table", $"dbo.{tableName}"),
                new SqlParameter("@Column", columnName)
            ],
            cancellationToken: cancellationToken);

        return result is int i && i == 1 || result is long l && l == 1;
    }
}
