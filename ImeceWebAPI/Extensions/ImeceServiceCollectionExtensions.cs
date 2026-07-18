using System.Reflection;
using Application;
using Application.Common.Storage;
using Core.Common;
using Core.DependencyInjection.Extensions;
using ImeceWebAPI.Infrastructure;
using ImeceWebAPI.Services;
using Infrastructure;
using Microsoft.Extensions.Hosting;

namespace ImeceWebAPI.Extensions;

/// <summary>
/// Imece uygulamasının tüm servis kayıtlarını tek bir yüksek seviyeli
/// composition root üzerinden toplar. Program.cs bu metodu çağırır; manuel
/// AddScoped satırları burada değil, convention/explicit yapıda yönetilir.
/// </summary>
public static class ImeceServiceCollectionExtensions
{
    public static IServiceCollection AddImeceApplication(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment,
        params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(environment);

        // Explicit (convention'a uygun olmayan) kayıtlar.
        services.AddApplication();
        services.AddInfrastructure();
        services.AddImeceFileStorage();

        // İstek iptali için merkezî, "yaşayan" token altyapısı.
        services.AddHttpContextAccessor();
        services.AddScoped<IRequestCancellation, HttpRequestCancellation>();

        // Denetim (audit) istek bağlamı: TraceId/IP/UserAgent/ClientApplication
        // güvenilir kaynaklardan. ClientApplication allow-list ile doğrulanır.
        services
            .AddOptions<ImeceWebAPI.Options.ClientApplicationOptions>()
            .Bind(configuration.GetSection(ImeceWebAPI.Options.ClientApplicationOptions.SectionName));
        services.AddScoped<Core.Auditing.IAuditRequestContext, ImeceWebAPI.Authentication.Context.HttpAuditRequestContext>();

        // Marker interface / attribute tabanlı otomatik kayıtlar.
        services.AddConventionRegisteredServices(
            configuration,
            options =>
            {
                options.ThrowOnDuplicate = true;
                options.ThrowOnMultipleImplementations = true;
                options.EnableRegistrationReport = environment.IsDevelopment();
            },
            assemblies);

        return services;
    }

    /// <summary>
    /// Dosya depolama sağlayıcısının explicit kaydı. Sağlayıcı seçimi
    /// backend tarafında belirlenir; convention taramasına bırakılmaz.
    /// </summary>
    private static IServiceCollection AddImeceFileStorage(
        this IServiceCollection services)
    {
        services.AddScoped<IFileStorageService, LocalFileStorageService>();

        return services;
    }
}
