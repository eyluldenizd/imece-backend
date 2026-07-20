using Infrastructure.Database.Connections;
using Infrastructure.Database.Options;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Database.Bootstrap;

public sealed class DatabaseBootstrapper : IDatabaseBootstrapper
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IDbExecutor _executor;
    private readonly IOptions<DatabaseOptions> _databaseOptions;
    private readonly IOptions<DatabaseSchemaOptions> _schemaOptions;
    private readonly ILogger<DatabaseBootstrapper> _logger;

    public DatabaseBootstrapper(
        IDbConnectionFactory connectionFactory,
        IDbExecutor executor,
        IOptions<DatabaseOptions> databaseOptions,
        IOptions<DatabaseSchemaOptions> schemaOptions,
        ILogger<DatabaseBootstrapper> logger)
    {
        _connectionFactory = connectionFactory;
        _executor = executor;
        _databaseOptions = databaseOptions;
        _schemaOptions = schemaOptions;
        _logger = logger;
    }

    public async Task<bool> EnsureDatabaseAsync(CancellationToken cancellationToken = default)
    {
        var dbOptions = _databaseOptions.Value;
        var schemaOptions = _schemaOptions.Value;

        var allowCreate = dbOptions.CreateIfMissing || schemaOptions.CreateDatabaseIfMissing;
        if (!allowCreate)
        {
            _logger.LogDebug("Veritabanı otomatik oluşturma kapalı.");
            return false;
        }

        var databaseName = dbOptions.DatabaseName;
        if (string.IsNullOrWhiteSpace(databaseName))
        {
            throw new InvalidOperationException("Database:DatabaseName yapılandırılmamış.");
        }

        await using var master =
            await _connectionFactory.OpenMasterConnectionAsync(cancellationToken);

        var exists = await _executor.ExecuteScalarAsync(
            master,
            """
            SELECT COUNT(1)
            FROM sys.databases
            WHERE name = @DatabaseName;
            """,
            parameters: [new SqlParameter("@DatabaseName", databaseName)],
            commandTimeoutSeconds: schemaOptions.CommandTimeoutSeconds,
            cancellationToken: cancellationToken);

        if (exists is int count && count > 0
            || exists is long longCount && longCount > 0)
        {
            return false;
        }

        // CREATE DATABASE cannot run inside a user transaction; execute directly.
        var safeName = databaseName.Replace("]", "]]", StringComparison.Ordinal);
        await _executor.ExecuteNonQueryAsync(
            master,
            $"IF DB_ID(N'{safeName}') IS NULL CREATE DATABASE [{safeName}];",
            commandTimeoutSeconds: schemaOptions.CommandTimeoutSeconds,
            cancellationToken: cancellationToken);

        _logger.LogInformation("Veritabanı oluşturuldu: {DatabaseName}", databaseName);
        return true;
    }
}
