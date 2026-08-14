using Application.Common.Execution;
using Core.Auditing;
using Core.Common.Execution;
using Infrastructure.Database.Audit;
using Infrastructure.Database.DataAccess;
using Infrastructure.Database.Options;
using ImeceWebAPI.Auditing;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ImeceWebAPI.Extensions;

/// <summary>
/// Otomatik denetim composition root: executor + SQL dekoratör + request middleware.
/// Writer/queue/service DatabaseExtensions'ta kayıtlıdır.
/// </summary>
public static class AuditExtensions
{
    public static IServiceCollection AddImeceAutomaticAuditing(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Options zaten DatabaseExtensions'ta bind edilmiş olabilir; idempotent yeniden bind.
        services
            .AddOptions<AuditOptions>()
            .Bind(configuration.GetSection(AuditOptions.SectionName));

        DecorateServiceExecutor(services);
        DecorateSqlDataAccess(services);

        return services;
    }

    public static WebApplication UseImeceRequestAuditing(this WebApplication app)
    {
        app.UseMiddleware<RequestAuditMiddleware>();
        return app;
    }

    private static void DecorateServiceExecutor(IServiceCollection services)
    {
        services.RemoveAll<IServiceExecutor>();
        services.AddScoped<ServiceExecutor>();
        services.AddScoped<IServiceExecutor>(sp =>
        {
            var inner = sp.GetRequiredService<ServiceExecutor>();
            return new AuditingServiceExecutor(
                inner,
                sp.GetRequiredService<IAuditService>(),
                sp.GetRequiredService<IAuditRequestContext>(),
                sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<AuditOptions>>(),
                sp.GetRequiredService<ILogger<AuditingServiceExecutor>>());
        });
    }

    private static void DecorateSqlDataAccess(IServiceCollection services)
    {
        services.RemoveAll<ISqlDataAccess>();
        services.AddScoped<SqlDataAccess>();
        services.AddScoped<ISqlDataAccess>(sp =>
        {
            var inner = sp.GetRequiredService<SqlDataAccess>();
            return new AuditingSqlDataAccess(
                inner,
                sp.GetRequiredService<IAuditService>(),
                sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<AuditOptions>>(),
                sp.GetRequiredService<ILogger<AuditingSqlDataAccess>>());
        });
    }
}
