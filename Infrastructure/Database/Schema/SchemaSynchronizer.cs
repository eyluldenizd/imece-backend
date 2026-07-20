using Infrastructure.Database.Connections;
using Infrastructure.Database.Options;
using Infrastructure.Database.Seeding;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Database.Schema;

public sealed class SchemaSynchronizer : ISchemaSynchronizer
{
    public const string HistoryTableName = "__ImeceSchemaHistory";

    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IDbExecutor _executor;
    private readonly ISchemaMetadataReader _metadataReader;
    private readonly ISchemaDiffer _differ;
    private readonly ISchemaScriptGenerator _scriptGenerator;
    private readonly IEnumerable<ISchemaDefinition> _definitions;
    private readonly ISystemDataSeeder _systemDataSeeder;
    private readonly IOptions<DatabaseSchemaOptions> _options;
    private readonly ILogger<SchemaSynchronizer> _logger;

    public SchemaSynchronizer(
        IDbConnectionFactory connectionFactory,
        IDbExecutor executor,
        ISchemaMetadataReader metadataReader,
        ISchemaDiffer differ,
        ISchemaScriptGenerator scriptGenerator,
        IEnumerable<ISchemaDefinition> definitions,
        ISystemDataSeeder systemDataSeeder,
        IOptions<DatabaseSchemaOptions> options,
        ILogger<SchemaSynchronizer> logger)
    {
        _connectionFactory = connectionFactory;
        _executor = executor;
        _metadataReader = metadataReader;
        _differ = differ;
        _scriptGenerator = scriptGenerator;
        _definitions = definitions;
        _systemDataSeeder = systemDataSeeder;
        _options = options;
        _logger = logger;
    }

    public async Task<SchemaSyncResult> SynchronizeAsync(
        CancellationToken cancellationToken = default)
    {
        var options = _options.Value;

        try
        {
            await using var connection =
                await _connectionFactory.OpenApplicationConnectionAsync(cancellationToken);

            await EnsureHistoryTableAsync(connection, options.CommandTimeoutSeconds, cancellationToken);

            var expectedTables = _definitions
                .SelectMany(d => d.Tables)
                .GroupBy(t => t.Name, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First())
                .ToList();

            // History table is internal; include it in expected for create-if-missing.
            expectedTables.Insert(0, CreateHistoryTableDefinition());

            var existing = await _metadataReader.ReadAsync(connection, cancellationToken: cancellationToken);
            var diff = _differ.Diff(expectedTables, existing);

            if (!diff.HasChanges)
            {
                _logger.LogInformation("Schema senkronizasyonu: değişiklik yok.");
                await SeedSystemDataAsync(connection, options, cancellationToken);
                return SchemaSyncResult.Success(0);
            }

            var safeChanges = diff.Changes
                .Where(c => !c.IsDestructive)
                .OrderBy(ChangePriority)
                .ToList();

            var destructiveChanges = diff.Changes.Where(c => c.IsDestructive).ToList();

            if (options.Mode == SchemaSyncMode.ValidateOnly)
            {
                _logger.LogWarning(
                    "Schema ValidateOnly: {Safe} güvenli, {Destructive} yıkıcı bekleyen değişiklik var.",
                    safeChanges.Count, destructiveChanges.Count);

                foreach (var change in diff.Changes)
                {
                    _logger.LogInformation("Bekleyen schema değişikliği: {Change}", change.Description);
                }

                return SchemaSyncResult.Success(0, diff.Changes.Count);
            }

            if (options.Mode == SchemaSyncMode.SafeApply
                || (options.Mode == SchemaSyncMode.DestructiveApply && !options.AllowDestructiveChanges))
            {
                var applied = await ApplyChangesAsync(
                    connection, safeChanges, options, cancellationToken);

                if (destructiveChanges.Count > 0)
                {
                    _logger.LogWarning(
                        "SafeApply: {Count} yıkıcı değişiklik atlandı (AllowDestructiveChanges=false veya SafeApply).",
                        destructiveChanges.Count);
                }

                await SeedSystemDataAsync(connection, options, cancellationToken);
                return SchemaSyncResult.Success(applied, destructiveChanges.Count);
            }

            // DestructiveApply with AllowDestructiveChanges
            var all = safeChanges.Concat(destructiveChanges).OrderBy(ChangePriority).ToList();
            var appliedAll = await ApplyChangesAsync(connection, all, options, cancellationToken);
            await SeedSystemDataAsync(connection, options, cancellationToken);
            return SchemaSyncResult.Success(appliedAll);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Schema senkronizasyonu başarısız.");
            return SchemaSyncResult.Failure(ex.Message);
        }
    }

    private async Task<int> ApplyChangesAsync(
        SqlConnection connection,
        IReadOnlyList<SchemaChange> changes,
        DatabaseSchemaOptions options,
        CancellationToken cancellationToken)
    {
        var applied = 0;

        foreach (var change in changes)
        {
            var sql = _scriptGenerator.Generate(change);
            if (options.LogGeneratedSql)
            {
                _logger.LogInformation("Schema SQL: {Sql}", sql);
            }

            await _executor.ExecuteNonQueryAsync(
                connection,
                sql,
                commandTimeoutSeconds: options.CommandTimeoutSeconds,
                cancellationToken: cancellationToken);

            await RecordHistoryAsync(
                connection,
                change,
                sql,
                options.CommandTimeoutSeconds,
                cancellationToken);

            applied++;
            _logger.LogInformation("Schema değişikliği uygulandı: {Change}", change.Description);
        }

        return applied;
    }

    private async Task SeedSystemDataAsync(
        SqlConnection connection,
        DatabaseSchemaOptions options,
        CancellationToken cancellationToken)
    {
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(cancellationToken);
        try
        {
            await _systemDataSeeder.SeedAsync(
                connection,
                transaction,
                options.CommandTimeoutSeconds,
                cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }

    private async Task EnsureHistoryTableAsync(
        SqlConnection connection,
        int commandTimeout,
        CancellationToken cancellationToken)
    {
        var sql = _scriptGenerator.GenerateCreateTable(CreateHistoryTableDefinition());
        await _executor.ExecuteNonQueryAsync(
            connection,
            sql,
            commandTimeoutSeconds: commandTimeout,
            cancellationToken: cancellationToken);
    }

    private async Task RecordHistoryAsync(
        SqlConnection connection,
        SchemaChange change,
        string sql,
        int commandTimeout,
        CancellationToken cancellationToken)
    {
        await _executor.ExecuteNonQueryAsync(
            connection,
            """
            INSERT INTO [dbo].[__ImeceSchemaHistory]
                (feature_name, change_kind, table_name, object_name, script_text, applied_at)
            VALUES
                (@FeatureName, @ChangeKind, @TableName, @ObjectName, @ScriptText, SYSUTCDATETIME());
            """,
            parameters:
            [
                new SqlParameter("@FeatureName", (object?)change.Table?.Name ?? change.TableName),
                new SqlParameter("@ChangeKind", change.Kind.ToString()),
                new SqlParameter("@TableName", change.TableName),
                new SqlParameter("@ObjectName", (object?)change.ObjectName ?? DBNull.Value),
                new SqlParameter("@ScriptText", sql.Length > 4000 ? sql[..4000] : sql)
            ],
            commandTimeoutSeconds: commandTimeout,
            cancellationToken: cancellationToken);
    }

    private static int ChangePriority(SchemaChange change) =>
        change.Kind switch
        {
            SchemaChangeKind.CreateTable => 0,
            SchemaChangeKind.AddColumn => 1,
            SchemaChangeKind.CreateIndex => 2,
            SchemaChangeKind.AddCheckConstraint => 3,
            SchemaChangeKind.AddForeignKey => 4,
            SchemaChangeKind.AlterColumn => 5,
            SchemaChangeKind.DropIndex => 6,
            SchemaChangeKind.DropColumn => 7,
            SchemaChangeKind.DropTable => 8,
            _ => 99
        };

    internal static TableDefinition CreateHistoryTableDefinition() =>
        new()
        {
            Name = HistoryTableName,
            Columns =
            [
                new ColumnDefinition
                {
                    Name = "history_id",
                    SqlType = "BIGINT",
                    IsIdentity = true,
                    IsPrimaryKey = true,
                    IsNullable = false
                },
                new ColumnDefinition
                {
                    Name = "feature_name",
                    SqlType = "NVARCHAR(128)",
                    IsNullable = false
                },
                new ColumnDefinition
                {
                    Name = "change_kind",
                    SqlType = "NVARCHAR(64)",
                    IsNullable = false
                },
                new ColumnDefinition
                {
                    Name = "table_name",
                    SqlType = "NVARCHAR(128)",
                    IsNullable = false
                },
                new ColumnDefinition
                {
                    Name = "object_name",
                    SqlType = "NVARCHAR(256)",
                    IsNullable = true
                },
                new ColumnDefinition
                {
                    Name = "script_text",
                    SqlType = "NVARCHAR(MAX)",
                    IsNullable = false
                },
                new ColumnDefinition
                {
                    Name = "applied_at",
                    SqlType = "DATETIME2",
                    IsNullable = false,
                    DefaultExpression = "SYSUTCDATETIME()"
                }
            ]
        };
}
