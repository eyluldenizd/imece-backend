using System.Reflection;
using Core.DependencyInjection.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Core.DependencyInjection.Extensions;

/// <summary>
/// Marker attribute / interface tabanlı otomatik DI kaydı. Bu turda güvenli
/// no-op tarama yapılır; mevcut explicit kayıtlar bozulmaz.
/// </summary>
public static class ConventionServiceCollectionExtensions
{
    public static IServiceCollection AddConventionRegisteredServices(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<ConventionRegistrationOptions>? configure = null,
        params Assembly[] assemblies)
    {
        var options = new ConventionRegistrationOptions();
        configure?.Invoke(options);

        var report = new ServiceRegistrationReport();

        foreach (var assembly in assemblies.Distinct())
        {
            // Şimdilik yalnızca rapor için assembly adını kaydet; gerçek
            // attribute tabanlı tarama sonraki turda genişletilebilir.
            report.Add(new ServiceRegistrationEntry
            {
                ServiceType = "(assembly-scan)",
                ImplementationType = assembly.GetName().Name ?? assembly.FullName ?? "?",
                Lifetime = "Scoped"
            });
        }

        if (options.EnableRegistrationReport)
        {
            services.TryAddSingleton(report);
        }

        return services;
    }
}
