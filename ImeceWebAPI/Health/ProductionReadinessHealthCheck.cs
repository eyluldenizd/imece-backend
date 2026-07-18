using ImeceWebAPI.Extensions;
using Infrastructure.Database.Connections;
using Infrastructure.Database.Options;
using Infrastructure.Database.Readiness;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace ImeceWebAPI.Health;

/// <summary>
/// Production hazırlık (readiness) sağlık kontrolü. Yapılandırma güvenliğini
/// (<see cref="ProductionReadinessEvaluator"/>) ve runtime DB durumunu (bağlantı,
/// beklenen DB adı) birleştirir. Yanıt secret veya detaylı DB yapısı İÇERMEZ;
/// yalnızca kontrol adı/geçti/kısa açıklama döner.
/// </summary>
public sealed class ProductionReadinessHealthCheck : IHealthCheck
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IHostEnvironment _environment;
    private readonly IOptions<ProductionSafetyOptions> _safetyOptions;
    private readonly IOptions<DatabaseOptions> _databaseOptions;
    private readonly IDbConnectionFactory _connectionFactory;

    public ProductionReadinessHealthCheck(
        IServiceProvider serviceProvider,
        IHostEnvironment environment,
        IOptions<ProductionSafetyOptions> safetyOptions,
        IOptions<DatabaseOptions> databaseOptions,
        IDbConnectionFactory connectionFactory)
    {
        _serviceProvider = serviceProvider;
        _environment = environment;
        _safetyOptions = safetyOptions;
        _databaseOptions = databaseOptions;
        _connectionFactory = connectionFactory;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var snapshot = DatabaseExtensions.BuildReadinessSnapshot(_serviceProvider, _environment);
        var configResult = ProductionReadinessEvaluator.Evaluate(_safetyOptions.Value, snapshot);

        var data = new Dictionary<string, object>();
        foreach (var check in configResult.Checks)
        {
            data[check.Name] = check.Passed ? "pass" : (check.Required ? "fail" : "warn");
        }

        var (dbConnected, dbNameMatches) = await CheckDatabaseAsync(cancellationToken);
        data["DatabaseConnection"] = dbConnected ? "pass" : "fail";
        data["ExpectedDatabaseName"] = dbNameMatches ? "pass" : "fail";

        var configSafe = configResult.IsSafe;
        var runtimeHealthy = dbConnected && dbNameMatches;

        if (configSafe && runtimeHealthy)
        {
            return HealthCheckResult.Healthy("Production readiness OK.", data);
        }

        if (!runtimeHealthy)
        {
            return HealthCheckResult.Unhealthy("Veritabanı erişimi/DB adı doğrulanamadı.", data: data);
        }

        // Yapılandırma güvensiz: FailStartup açıksa Unhealthy, değilse Degraded.
        return _safetyOptions.Value.FailStartupOnUnsafeConfiguration
            ? HealthCheckResult.Unhealthy("Güvenli olmayan production yapılandırması.", data: data)
            : HealthCheckResult.Degraded("Production yapılandırması önerilenden zayıf.", data: data);
    }

    private async Task<(bool Connected, bool NameMatches)> CheckDatabaseAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            await using var connection = await _connectionFactory.OpenApplicationConnectionAsync(cancellationToken);
            var actualDatabase = connection.Database;
            var expected = _databaseOptions.Value.DatabaseName;
            var matches = string.Equals(actualDatabase, expected, StringComparison.OrdinalIgnoreCase);
            return (true, matches);
        }
        catch (SqlException)
        {
            return (false, false);
        }
    }
}
