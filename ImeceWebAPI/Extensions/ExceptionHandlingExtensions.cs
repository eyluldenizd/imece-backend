using ImeceWebAPI.Errors;
using ImeceWebAPI.Options;

namespace ImeceWebAPI.Extensions;

/// <summary>
/// Merkezî exception handling'in servis kaydı ve pipeline kurulumu.
/// Program.cs bu extension'lar üzerinden sade kalır.
/// </summary>
public static class ExceptionHandlingExtensions
{
    public static IServiceCollection AddImeceExceptionHandling(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        services
            .AddOptions<ExceptionHandlingOptions>()
            .Bind(configuration.GetSection(ExceptionHandlingOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();

        return services;
    }

    public static WebApplication UseImeceExceptionHandling(
        this WebApplication app)
    {
        // IExceptionHandler tabanlı yapı UseExceptionHandler ile devreye girer.
        app.UseExceptionHandler();

        return app;
    }
}
