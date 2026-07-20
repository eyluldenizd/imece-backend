using Infrastructure.Database.Connections;
using Infrastructure.Database.Options;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Database.Seeding;

public sealed class DevelopmentDatabaseResetter : IDevelopmentDatabaseResetter
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IDbExecutor _executor;
    private readonly IHostEnvironment _environment;
    private readonly IOptions<DatabaseOptions> _databaseOptions;
    private readonly IOptions<DatabaseSchemaOptions> _schemaOptions;
    private readonly ILogger<DevelopmentDatabaseResetter> _logger;

    public DevelopmentDatabaseResetter(
        IDbConnectionFactory connectionFactory,
        IDbExecutor executor,
        IHostEnvironment environment,
        IOptions<DatabaseOptions> databaseOptions,
        IOptions<DatabaseSchemaOptions> schemaOptions,
        ILogger<DevelopmentDatabaseResetter> logger)
    {
        _connectionFactory = connectionFactory;
        _executor = executor;
        _environment = environment;
        _databaseOptions = databaseOptions;
        _schemaOptions = schemaOptions;
        _logger = logger;
    }

    public async Task<DevelopmentDatabaseResetResult> TryResetAsync(
        CancellationToken cancellationToken = default)
    {
        var options = _schemaOptions.Value;

        if (!options.ResetDatabase)
        {
            return new DevelopmentDatabaseResetResult
            {
                Outcome = DevelopmentDatabaseResetOutcome.NotRequested,
                Detail = "ResetDatabase=false"
            };
        }

        if (_environment.IsProduction())
        {
            _logger.LogWarning(
                "ResetDatabase=true production ortamında yok sayıldı; veritabanı silinmedi.");
            return new DevelopmentDatabaseResetResult
            {
                Outcome = DevelopmentDatabaseResetOutcome.NotAllowed,
                Detail = "Production ortamında reset yasak."
            };
        }

        var isDevelopmentOrTest =
            _environment.IsDevelopment() || _environment.IsEnvironment("Test");

        if (!isDevelopmentOrTest)
        {
            return new DevelopmentDatabaseResetResult
            {
                Outcome = DevelopmentDatabaseResetOutcome.NotAllowed,
                Detail = "Reset yalnızca Development/Test ortamlarında çalışır."
            };
        }

        var databaseName = _databaseOptions.Value.DatabaseName;
        var allowedNames = options.AllowedResetDatabaseNames ?? [];

        if (allowedNames.Length == 0 ||
            !allowedNames.Any(n => string.Equals(n, databaseName, StringComparison.OrdinalIgnoreCase)))
        {
            _logger.LogWarning(
                "ResetDatabase=true ancak DB adı '{DatabaseName}' AllowedResetDatabaseNames listesinde değil; reset atlandı.",
                databaseName);
            return new DevelopmentDatabaseResetResult
            {
                Outcome = DevelopmentDatabaseResetOutcome.NotAllowed,
                Detail = $"DB adı izin listesinde değil: {databaseName}"
            };
        }

        var timeout = options.CommandTimeoutSeconds;

        await using var connection =
            await _connectionFactory.OpenApplicationConnectionAsync(cancellationToken);

        var appliedVersion = await DevelopmentSeedState.GetAppliedVersionAsync(
            _executor,
            connection,
            timeout,
            cancellationToken: cancellationToken);

        if (string.Equals(appliedVersion, options.DevelopmentSeedVersion, StringComparison.Ordinal))
        {
            _logger.LogInformation(
                "Development seed sürümü '{SeedVersion}' zaten uygulanmış; veritabanı wipe atlandı.",
                options.DevelopmentSeedVersion);
            return new DevelopmentDatabaseResetResult
            {
                Outcome = DevelopmentDatabaseResetOutcome.SkippedAlreadyApplied,
                Detail = options.DevelopmentSeedVersion
            };
        }

        _logger.LogWarning(
            "Development veritabanı wipe başlatılıyor (DB={DatabaseName}, hedef sürüm={SeedVersion}).",
            databaseName,
            options.DevelopmentSeedVersion);

        var transaction = connection.BeginTransaction();

        try
        {
            await WipeApplicationDataAsync(connection, transaction, timeout, cancellationToken);
            await ReseedIdentitiesAsync(_executor, connection, transaction, timeout, cancellationToken);
            await DevelopmentSeedState.ClearAsync(
                _executor,
                connection,
                timeout,
                transaction,
                cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            _logger.LogWarning(
                "Development veritabanı wipe tamamlandı (DB={DatabaseName}).",
                databaseName);

            return new DevelopmentDatabaseResetResult
            {
                Outcome = DevelopmentDatabaseResetOutcome.Wiped,
                Detail = options.DevelopmentSeedVersion
            };
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
        finally
        {
            await transaction.DisposeAsync();
        }
    }

    private async Task WipeApplicationDataAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        int commandTimeoutSeconds,
        CancellationToken cancellationToken)
    {
        await _executor.ExecuteNonQueryAsync(
            connection,
            """
            IF OBJECT_ID(N'[dbo].[service_route_stops]', N'U') IS NOT NULL
                DELETE FROM [dbo].[service_route_stops];

            IF OBJECT_ID(N'[dbo].[service_routes]', N'U') IS NOT NULL
                DELETE FROM [dbo].[service_routes];

            IF OBJECT_ID(N'[dbo].[weekly_menu_items]', N'U') IS NOT NULL
                DELETE FROM [dbo].[weekly_menu_items];

            IF OBJECT_ID(N'[dbo].[weekly_menu_entries]', N'U') IS NOT NULL
                DELETE FROM [dbo].[weekly_menu_entries];

            IF OBJECT_ID(N'[dbo].[weekly_menus]', N'U') IS NOT NULL
                DELETE FROM [dbo].[weekly_menus];

            IF OBJECT_ID(N'[dbo].[reservations]', N'U') IS NOT NULL
                DELETE FROM [dbo].[reservations];

            IF OBJECT_ID(N'[dbo].[meeting_rooms]', N'U') IS NOT NULL
                DELETE FROM [dbo].[meeting_rooms];

            IF OBJECT_ID(N'[dbo].[content_company_targets]', N'U') IS NOT NULL
                DELETE FROM [dbo].[content_company_targets];

            IF OBJECT_ID(N'[dbo].[media_folders]', N'U') IS NOT NULL
            BEGIN
                UPDATE [dbo].[media_folders]
                SET [cover_media_file_id] = NULL,
                    [parent_folder_id] = NULL,
                    [event_id] = NULL;
            END

            IF OBJECT_ID(N'[dbo].[media_files]', N'U') IS NOT NULL
                DELETE FROM [dbo].[media_files];

            IF OBJECT_ID(N'[dbo].[media_folders]', N'U') IS NOT NULL
                DELETE FROM [dbo].[media_folders];

            IF OBJECT_ID(N'[dbo].[events]', N'U') IS NOT NULL
                DELETE FROM [dbo].[events];

            IF OBJECT_ID(N'[dbo].[announcements]', N'U') IS NOT NULL
                DELETE FROM [dbo].[announcements];

            IF OBJECT_ID(N'[dbo].[social_activities]', N'U') IS NOT NULL
                DELETE FROM [dbo].[social_activities];

            IF OBJECT_ID(N'[dbo].[campaigns]', N'U') IS NOT NULL
                DELETE FROM [dbo].[campaigns];

            IF OBJECT_ID(N'[dbo].[e_cards]', N'U') IS NOT NULL
                DELETE FROM [dbo].[e_cards];

            IF OBJECT_ID(N'[dbo].[emergency_numbers]', N'U') IS NOT NULL
                DELETE FROM [dbo].[emergency_numbers];

            IF OBJECT_ID(N'[dbo].[communication_channels]', N'U') IS NOT NULL
                DELETE FROM [dbo].[communication_channels];

            IF OBJECT_ID(N'[dbo].[corporate_apps]', N'U') IS NOT NULL
                DELETE FROM [dbo].[corporate_apps];

            IF OBJECT_ID(N'[dbo].[services]', N'U') IS NOT NULL
                DELETE FROM [dbo].[services];

            IF OBJECT_ID(N'[dbo].[today_in_history]', N'U') IS NOT NULL
                DELETE FROM [dbo].[today_in_history];

            IF OBJECT_ID(N'[dbo].[dishes]', N'U') IS NOT NULL
                DELETE FROM [dbo].[dishes];

            IF OBJECT_ID(N'[dbo].[weeks]', N'U') IS NOT NULL
                DELETE FROM [dbo].[weeks];

            IF OBJECT_ID(N'[dbo].[upcoming_events]', N'U') IS NOT NULL
                DELETE FROM [dbo].[upcoming_events];

            IF OBJECT_ID(N'[dbo].[service_locations]', N'U') IS NOT NULL
                DELETE FROM [dbo].[service_locations];

            IF OBJECT_ID(N'[dbo].[audit_log]', N'U') IS NOT NULL
                DELETE FROM [dbo].[audit_log];

            IF OBJECT_ID(N'[dbo].[external_user_identities]', N'U') IS NOT NULL
                DELETE FROM [dbo].[external_user_identities];

            IF OBJECT_ID(N'[dbo].[user_company_roles]', N'U') IS NOT NULL
                DELETE FROM [dbo].[user_company_roles];

            IF OBJECT_ID(N'[dbo].[users]', N'U') IS NOT NULL
                DELETE FROM [dbo].[users];

            IF OBJECT_ID(N'[dbo].[departments]', N'U') IS NOT NULL
                DELETE FROM [dbo].[departments];

            IF OBJECT_ID(N'[dbo].[branches]', N'U') IS NOT NULL
                DELETE FROM [dbo].[branches];

            IF OBJECT_ID(N'[dbo].[role_permissions]', N'U') IS NOT NULL
                DELETE FROM [dbo].[role_permissions];

            IF OBJECT_ID(N'[dbo].[companies]', N'U') IS NOT NULL
                DELETE FROM [dbo].[companies];

            IF OBJECT_ID(N'[dbo].[roles]', N'U') IS NOT NULL
                DELETE FROM [dbo].[roles];

            IF OBJECT_ID(N'[dbo].[permissions]', N'U') IS NOT NULL
                DELETE FROM [dbo].[permissions];
            """,
            transaction: transaction,
            commandTimeoutSeconds: commandTimeoutSeconds,
            cancellationToken: cancellationToken);
    }

    private static async Task ReseedIdentitiesAsync(
        IDbExecutor executor,
        SqlConnection connection,
        SqlTransaction transaction,
        int commandTimeoutSeconds,
        CancellationToken cancellationToken)
    {
        string[] identityTables =
        [
            "permissions",
            "roles",
            "companies",
            "branches",
            "departments",
            "users",
            "user_company_roles",
            "external_user_identities",
            "meeting_rooms",
            "reservations",
            "dishes",
            "weeks",
            "weekly_menus",
            "weekly_menu_items",
            "weekly_menu_entries",
            "media_folders",
            "media_files",
            "announcements",
            "events",
            "social_activities",
            "campaigns",
            "e_cards",
            "emergency_numbers",
            "communication_channels",
            "corporate_apps",
            "services",
            "today_in_history",
            "service_locations",
            "service_routes",
            "service_route_stops",
            "content_company_targets",
            "audit_log"
        ];

        foreach (var table in identityTables)
        {
            await executor.ExecuteNonQueryAsync(
                connection,
                $"""
                 IF OBJECT_ID(N'[dbo].[{table}]', N'U') IS NOT NULL
                 BEGIN
                     DBCC CHECKIDENT (N'[dbo].[{table}]', RESEED, 0);
                 END
                 """,
                transaction: transaction,
                commandTimeoutSeconds: commandTimeoutSeconds,
                cancellationToken: cancellationToken);
        }
    }
}
