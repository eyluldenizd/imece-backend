using Core.Directory;
using ImeceWebAPI.Health;
using Infrastructure.Authentication.Directory;
using Infrastructure.Database.Audit;
using Infrastructure.Database.Backfill;
using Infrastructure.Database.Bootstrap;
using Infrastructure.Database.Connections;
using Infrastructure.Database.Hardening;
using Infrastructure.Database.Options;
using Infrastructure.Database.Readiness;
using Infrastructure.Database.Schema;
using Infrastructure.Database.Schema.Definitions;
using Infrastructure.Database.Seeding;
using Microsoft.Extensions.Options;

namespace ImeceWebAPI.Extensions;

/// <summary>
/// Veritabanı bağlantı profilleri, schema senkronizasyon motoru ve startup
/// initializer'ının kurulumu. Motor migration kullanmaz; sys.* katalogları ile
/// beklenen şemayı karşılaştırıp güvenli/idempotent uygular.
/// </summary>
public static class DatabaseExtensions
{
    public static IServiceCollection AddImeceDatabase(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services
            .AddOptions<DatabaseOptions>()
            .Bind(configuration.GetSection(DatabaseOptions.SectionName));

        // Şema seçenekleri: ortam bazlı güvenli varsayılanlar + configuration override.
        var isDevelopment = environment.IsDevelopment();
        var configuredMode = configuration
            .GetSection(DatabaseSchemaOptions.SectionName)
            .GetValue<SchemaSyncMode?>(nameof(DatabaseSchemaOptions.Mode));

        services
            .AddOptions<DatabaseSchemaOptions>()
            .Bind(configuration.GetSection(DatabaseSchemaOptions.SectionName))
            .PostConfigure(options =>
            {
                // Mode configuration'da yoksa: Development→SafeApply, Production→ValidateOnly.
                if (configuredMode is null)
                {
                    options.Mode = isDevelopment ? SchemaSyncMode.SafeApply : SchemaSyncMode.ValidateOnly;
                }

                // Güvenlik: her iki ortamda da destructive varsayılan olarak kapalı.
                if (!isDevelopment && options.Mode == SchemaSyncMode.DestructiveApply)
                {
                    options.AllowDestructiveChanges = options.AllowDestructiveChanges && true;
                }

                if (options.AllowedResetDatabaseNames is null or { Length: 0 })
                {
                    options.AllowedResetDatabaseNames = ["SankoImece", "SankoImece_Test"];
                }
            });

        // Bağlantı ve düşük seviyeli yürütücüler (stateless → singleton).
        services.AddSingleton<IConnectionStringFactory, ConnectionStringFactory>();
        services.AddSingleton<IDbConnectionFactory, SqlDbConnectionFactory>();
        services.AddSingleton<IDbExecutor, SqlDbExecutor>();
        services.AddSingleton<ISqlExceptionTranslator, SqlExceptionTranslator>();

        // Schema motoru.
        services.AddSingleton<ISchemaScriptGenerator, SqlServerSchemaScriptGenerator>();
        services.AddSingleton<ISchemaDiffer, SchemaDiffer>();
        services.AddSingleton<ISchemaMetadataReader, SqlSchemaMetadataReader>();
        services.AddSingleton<ISystemDataSeeder, SystemDataSeeder>();
        services.AddSingleton<IRealisticDevelopmentContentSeeder, RealisticDevelopmentContentSeeder>();
        services.AddSingleton<IDevelopmentDataSeeder, DevelopmentDataSeeder>();
        services.AddSingleton<IDevelopmentDatabaseResetter, DevelopmentDatabaseResetter>();
        services.AddSingleton<IDatabaseBootstrapper, DatabaseBootstrapper>();
        services.AddSingleton<ISchemaSynchronizer, SchemaSynchronizer>();

        // Feature bazlı beklenen şema tanımları (IEnumerable<ISchemaDefinition>).
        services.AddSingleton<ISchemaDefinition, CompanySchemaDefinition>();
        services.AddSingleton<ISchemaDefinition, IdentitySchemaDefinition>();
        services.AddSingleton<ISchemaDefinition, AuthorizationSchemaDefinition>();
        services.AddSingleton<ISchemaDefinition, AuditSchemaDefinition>();
        services.AddSingleton<ISchemaDefinition, CompanyScopedContentSchemaDefinition>();
        services.AddSingleton<ISchemaDefinition, MealMenuSchemaDefinition>();
        services.AddSingleton<ISchemaDefinition, ServiceTransportSchemaDefinition>();
        services.AddSingleton<ISchemaDefinition, GlobalContentSchemaDefinition>();
        services.AddSingleton<ISchemaDefinition, MultiCompanyTargetedContentSchemaDefinition>();

        // Audit altyapısı: writer (singleton), sanitizer (singleton), merkezi
        // audit service (scoped — kullanıcı/şirket/istek bağlamına bağlı).
        services.AddSingleton<IAuditLogWriter, SqlAuditLogWriter>();
        services.AddSingleton<Core.Auditing.IAuditValueSanitizer, AuditValueSanitizer>();
        services
            .AddOptions<AuditOptions>()
            .Bind(configuration.GetSection(AuditOptions.SectionName));
        services.AddScoped<Core.Auditing.IAuditService, AuditService>();

        // İçerik izolasyonu davranışı (Enforce) ve backfill süreci.
        services
            .AddOptions<ContentIsolationOptions>()
            .Bind(configuration.GetSection(ContentIsolationOptions.SectionName));
        services
            .AddOptions<ContentScopeBackfillOptions>()
            .Bind(configuration.GetSection(ContentScopeBackfillOptions.SectionName));
        services.AddSingleton<IContentScopeBackfillService, SqlContentScopeBackfillService>();

        // Backfill sonrası şema sıkılaştırma (nullable → NOT NULL/CHECK). Güvenli
        // varsayılan: kapalı + yalnızca doğrulama; apply açık opt-in ister.
        services
            .AddOptions<SchemaHardeningOptions>()
            .Bind(configuration.GetSection(SchemaHardeningOptions.SectionName));
        services.AddSingleton<ISchemaHardeningValidator, SqlSchemaHardeningValidator>();

        // Production güvenli yapılandırma gereksinimleri + readiness sağlık kontrolü.
        services
            .AddOptions<ProductionSafetyOptions>()
            .Bind(configuration.GetSection(ProductionSafetyOptions.SectionName));
        services.AddHealthChecks()
            .AddCheck<ProductionReadinessHealthCheck>(
                "production-readiness",
                tags: ["ready"]);

        return services;
    }

    /// <summary>
    /// Yapılandırma tabanlı production readiness anlık görüntüsünü DI'dan toplar
    /// (secret içermez). Runtime DB kontrolleri sağlık kontrolünde eklenir.
    /// </summary>
    public static ProductionReadinessSnapshot BuildReadinessSnapshot(
        IServiceProvider provider,
        IHostEnvironment environment)
    {
        var isolation = provider.GetRequiredService<IOptions<ContentIsolationOptions>>().Value;
        var schema = provider.GetRequiredService<IOptions<DatabaseSchemaOptions>>().Value;
        var audit = provider.GetRequiredService<IOptions<AuditOptions>>().Value;
        var directory = provider.GetService<IDirectoryUserService>();

        var sqlBackedDirectory = directory is not null and not DevelopmentDirectoryUserService;

        return new ProductionReadinessSnapshot
        {
            IsProduction = environment.IsProduction(),
            ContentIsolationEnforced = isolation.Enforce,
            SchemaMode = schema.Mode,
            AuditEnabled = audit.Enabled,
            SqlBackedDirectoryProvider = sqlBackedDirectory,
            DevelopmentSeedEnabled = schema.SeedDevelopmentData,
            ResetDatabaseEnabled = schema.ResetDatabase
        };
    }

    /// <summary>
    /// Startup'ta (Enabled ise) DB bootstrap + schema senkronizasyonu + development
    /// seed çalıştırır. Request cancellation değil, ApplicationStopping token kullanır.
    /// </summary>
    public static async Task InitializeImeceDatabaseAsync(
        this WebApplication app)
    {
        var options = app.Services.GetRequiredService<IOptions<DatabaseSchemaOptions>>().Value;
        var logger = app.Services.GetRequiredService<ILoggerFactory>()
            .CreateLogger("ImeceWebAPI.Database.Initializer");

        if (!options.Enabled)
        {
            logger.LogInformation("Schema senkronizasyonu devre dışı (DatabaseSchema:Enabled=false).");
            return;
        }

        var cancellationToken = app.Lifetime.ApplicationStopping;

        using var scope = app.Services.CreateScope();
        var provider = scope.ServiceProvider;

        try
        {
            var bootstrapper = provider.GetRequiredService<IDatabaseBootstrapper>();
            var created = await bootstrapper.EnsureDatabaseAsync(cancellationToken);
            if (created)
            {
                logger.LogInformation("Veritabanı startup'ta oluşturuldu.");
            }

            var resetter = provider.GetRequiredService<IDevelopmentDatabaseResetter>();
            var resetResult = await resetter.TryResetAsync(cancellationToken);
            if (resetResult.Outcome == DevelopmentDatabaseResetOutcome.Wiped)
            {
                logger.LogWarning("Development veritabanı wipe tamamlandı: {Detail}", resetResult.Detail);
            }
            else if (resetResult.Outcome == DevelopmentDatabaseResetOutcome.SkippedAlreadyApplied)
            {
                logger.LogInformation("Development DB reset atlandı: {Detail}", resetResult.Detail);
            }

            var synchronizer = provider.GetRequiredService<ISchemaSynchronizer>();
            var result = await synchronizer.SynchronizeAsync(cancellationToken);

            if (!result.Succeeded)
            {
                if (options.FailApplicationOnError)
                {
                    throw new InvalidOperationException(
                        $"Schema senkronizasyonu başarısız: {result.ErrorMessage}");
                }

                logger.LogError("Schema senkronizasyonu başarısız (uygulama devam ediyor): {Error}",
                    result.ErrorMessage);
                return;
            }

            await RunContentScopeBackfillAsync(provider, logger, cancellationToken);

            await RunSchemaHardeningAsync(provider, logger, cancellationToken);

            await SeedDevelopmentDataAsync(app, provider, options, logger, cancellationToken);
        }
        catch (Exception ex) when (!options.FailApplicationOnError)
        {
            logger.LogError(ex, "Veritabanı initialize edilemedi (uygulama devam ediyor).");
        }
    }

    /// <summary>
    /// Şirket atanmamış içerik için backfill sürecini çalıştırır. Varsayılan
    /// güvenlidir (kapalı/dry-run). FailOnUnassignedContent açıkken atanmamış
    /// içerik kalırsa uygulama başlatma engellenir (izolasyon zorlanmadan önce).
    /// </summary>
    private static async Task RunContentScopeBackfillAsync(
        IServiceProvider provider,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var backfillOptions = provider
            .GetRequiredService<IOptions<ContentScopeBackfillOptions>>().Value;

        if (!backfillOptions.Enabled)
        {
            return;
        }

        var backfill = provider.GetRequiredService<IContentScopeBackfillService>();
        var report = await backfill.RunAsync(cancellationToken);

        logger.LogInformation(
            "İçerik backfill raporu: DryRun={DryRun} VarsayılanŞirket={Company} AtanmamışKalan={Remaining}.",
            report.DryRun, report.DefaultCompanyId, report.TotalUnassignedAfter);

        if (backfillOptions.FailOnUnassignedContent && report.HasUnassignedContent)
        {
            throw new InvalidOperationException(
                $"Şirkete atanmamış içerik mevcut ({report.TotalUnassignedAfter} satır). " +
                "İzolasyon zorlanmadan önce backfill tamamlanmalı (ContentScopeBackfill).");
        }
    }

    /// <summary>
    /// Backfill sonrası şema sıkılaştırma doğrulaması. Varsayılan güvenlidir
    /// (kapalı / yalnızca doğrulama). Apply yalnızca açık opt-in ve tüm
    /// doğrulamalar geçtiğinde çalışır. İhlal varken FailOnViolation açıksa
    /// uygulama başlatma engellenir.
    /// </summary>
    private static async Task RunSchemaHardeningAsync(
        IServiceProvider provider,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var hardeningOptions = provider
            .GetRequiredService<IOptions<SchemaHardeningOptions>>().Value;

        if (!hardeningOptions.Enabled)
        {
            return;
        }

        var validator = provider.GetRequiredService<ISchemaHardeningValidator>();
        var report = await validator.RunAsync(cancellationToken);

        logger.LogInformation(
            "Schema hardening raporu: ValidateOnly={ValidateOnly} Applyİstendi={Apply} Toplamİhlal={Total}.",
            report.ValidateOnly, report.ApplyRequested, report.TotalViolations);

        if (hardeningOptions.FailOnViolation && report.HasViolations)
        {
            throw new InvalidOperationException(
                $"Şema sıkılaştırma ihlalleri mevcut ({report.TotalViolations} kayıt). " +
                "NOT NULL/CHECK uygulanmadan önce backfill/temizlik tamamlanmalı (SchemaHardening).");
        }
    }

    /// <summary>
    /// Production güvenli yapılandırmayı değerlendirir; güvensizse loglar ve
    /// (FailStartupOnUnsafeConfiguration açıksa) başlatmayı durdurur. Development
    /// davranışını bozmaz.
    /// </summary>
    public static void EnsureProductionSafety(
        this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var provider = scope.ServiceProvider;

        var options = provider.GetRequiredService<IOptions<ProductionSafetyOptions>>().Value;
        var snapshot = BuildReadinessSnapshot(provider, app.Environment);
        var result = ProductionReadinessEvaluator.Evaluate(options, snapshot);

        var logger = app.Services.GetRequiredService<ILoggerFactory>()
            .CreateLogger("ImeceWebAPI.ProductionSafety");

        foreach (var check in result.Checks.Where(c => !c.Passed && c.Required))
        {
            logger.LogWarning("Production güvenlik kontrolü başarısız: {Check} — {Detail}",
                check.Name, check.Detail);
        }

        if (!result.IsSafe && options.FailStartupOnUnsafeConfiguration)
        {
            throw new InvalidOperationException(
                "Production için güvenli olmayan yapılandırma tespit edildi. " +
                "Ayrıntılar log'da (ProductionSafety). Başlatma durduruldu " +
                "(ProductionSafety:FailStartupOnUnsafeConfiguration=true).");
        }
    }

    private static async Task SeedDevelopmentDataAsync(
        WebApplication app,
        IServiceProvider provider,
        DatabaseSchemaOptions options,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var isDevelopmentOrTest =
            app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Test");

        if (!DevelopmentSeedPolicy.ShouldSeed(isDevelopmentOrTest, options.SeedDevelopmentData))
        {
            return;
        }

        var connectionFactory = provider.GetRequiredService<IDbConnectionFactory>();
        var seeder = provider.GetRequiredService<IDevelopmentDataSeeder>();

        await using var connection = await connectionFactory.OpenApplicationConnectionAsync(cancellationToken);
        var transaction = (Microsoft.Data.SqlClient.SqlTransaction)connection.BeginTransaction();

        try
        {
            await seeder.SeedAsync(connection, transaction, options.CommandTimeoutSeconds, cancellationToken);

            if (options.SeedRealisticContent || options.SeedDevelopmentData)
            {
                await DevelopmentSeedState.WriteAppliedVersionAsync(
                    provider.GetRequiredService<IDbExecutor>(),
                    connection,
                    options.DevelopmentSeedVersion,
                    options.CommandTimeoutSeconds,
                    transaction,
                    cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
            logger.LogInformation("Development seed uygulandı.");
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
}
