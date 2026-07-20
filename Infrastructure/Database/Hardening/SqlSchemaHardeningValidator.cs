using Infrastructure.Database.Connections;
using Infrastructure.Database.Options;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Database.Hardening;

public sealed class SqlSchemaHardeningValidator : ISchemaHardeningValidator
{
    private static readonly (string Table, string Column)[] RequiredCompanyColumns =
    [
        ("announcements", "company_id"),
        ("events", "company_id"),
        ("weekly_menu_entries", "company_id"),
        ("reservations", "company_id")
    ];

    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IDbExecutor _executor;
    private readonly IOptions<SchemaHardeningOptions> _options;
    private readonly ILogger<SqlSchemaHardeningValidator> _logger;

    public SqlSchemaHardeningValidator(
        IDbConnectionFactory connectionFactory,
        IDbExecutor executor,
        IOptions<SchemaHardeningOptions> options,
        ILogger<SqlSchemaHardeningValidator> logger)
    {
        _connectionFactory = connectionFactory;
        _executor = executor;
        _options = options;
        _logger = logger;
    }

    public async Task<SchemaHardeningReport> RunAsync(
        CancellationToken cancellationToken = default)
    {
        var options = _options.Value;
        await using var connection =
            await _connectionFactory.OpenApplicationConnectionAsync(cancellationToken);

        var details = new List<string>();
        var violations = 0;

        foreach (var (table, column) in RequiredCompanyColumns)
        {
            var tableExists = await _executor.ExecuteScalarAsync(
                connection,
                "SELECT CASE WHEN OBJECT_ID(@Name, N'U') IS NULL THEN 0 ELSE 1 END;",
                parameters: [new SqlParameter("@Name", $"dbo.{table}")],
                cancellationToken: cancellationToken);

            if (tableExists is 0 or 0L)
            {
                continue;
            }

            var nullCount = await _executor.ExecuteScalarAsync(
                connection,
                $"""
                SELECT COUNT(1)
                FROM [dbo].[{table}]
                WHERE [{column}] IS NULL;
                """,
                cancellationToken: cancellationToken);

            var nulls = nullCount switch
            {
                int i => i,
                long l => (int)l,
                _ => 0
            };

            if (nulls > 0)
            {
                violations += nulls;
                details.Add($"{table}.{column}: {nulls} NULL satır");
            }

            var isNullable = await _executor.ExecuteScalarAsync(
                connection,
                """
                SELECT c.is_nullable
                FROM sys.columns AS c
                INNER JOIN sys.tables AS t ON t.object_id = c.object_id
                INNER JOIN sys.schemas AS s ON s.schema_id = t.schema_id
                WHERE s.name = N'dbo'
                  AND t.name = @Table
                  AND c.name = @Column;
                """,
                parameters:
                [
                    new SqlParameter("@Table", table),
                    new SqlParameter("@Column", column)
                ],
                cancellationToken: cancellationToken);

            if (isNullable is true || isNullable is bool b && b)
            {
                details.Add($"{table}.{column}: kolon hâlâ NULLABLE");
                if (nulls == 0 && options.Apply && !options.ValidateOnly)
                {
                    await _executor.ExecuteNonQueryAsync(
                        connection,
                        $"""
                        ALTER TABLE [dbo].[{table}]
                        ALTER COLUMN [{column}] INT NOT NULL;
                        """,
                        cancellationToken: cancellationToken);
                    _logger.LogInformation(
                        "Schema hardening uygulandı: {Table}.{Column} → NOT NULL",
                        table, column);
                }
                else if (nulls == 0)
                {
                    // Schema-level nullable without data nulls counts as a soft violation
                    // until Apply is requested.
                    violations++;
                }
            }
        }

        _logger.LogInformation(
            "Schema hardening: ValidateOnly={ValidateOnly} Apply={Apply} Violations={Violations}",
            options.ValidateOnly, options.Apply, violations);

        return new SchemaHardeningReport
        {
            ValidateOnly = options.ValidateOnly,
            ApplyRequested = options.Apply,
            TotalViolations = violations,
            Details = details
        };
    }
}
