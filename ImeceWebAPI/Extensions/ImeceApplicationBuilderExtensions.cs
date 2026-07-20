using Core.DependencyInjection.Models;

namespace ImeceWebAPI.Extensions;

/// <summary>
/// Uygulama başlangıcında convention-based servis kayıt raporunu Development
/// ortamında Debug seviyesinde loglar. Production'da rapor üretilmez ve
/// loglanmaz; hassas configuration bilgisi rapora dahil edilmez.
/// </summary>
public static class ImeceApplicationBuilderExtensions
{
    public static WebApplication UseImeceServiceRegistrationReport(
        this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            return app;
        }

        var report = app.Services.GetService<ServiceRegistrationReport>();
        if (report is null)
        {
            return app;
        }

        var logger = app.Services
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("Imece.DependencyInjection");

        logger.LogDebug(
            "Convention-based service registrations ({Count}):{NewLine}{Report}",
            report.Count,
            Environment.NewLine,
            report.ToFormattedTable());

        return app;
    }
}
