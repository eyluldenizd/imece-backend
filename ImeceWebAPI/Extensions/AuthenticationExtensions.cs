using Core.Authentication;
using Core.Authorization;
using Core.Directory;
using ImeceWebAPI.Authentication;
using ImeceWebAPI.Authentication.Authorization;
using ImeceWebAPI.Authentication.Context;
using ImeceWebAPI.Authentication.Options;
using Infrastructure.Authentication;
using Infrastructure.Authentication.Directory;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace ImeceWebAPI.Extensions;

/// <summary>
/// Authentication ve application authorization altyapısının kurulumu.
/// Provider seçimi configuration (<c>Authentication:Mode</c>) ile yapılır;
/// üst katmanlar (controller, service, repository) yalnızca
/// <see cref="ICurrentUser"/>/<see cref="ICompanyContext"/> soyutlamalarına
/// bağlıdır. Provider değişimi bu extension + configuration ile sınırlıdır.
/// </summary>
public static class AuthenticationExtensions
{
    public static IServiceCollection AddImeceAuthentication(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services
            .AddOptions<ImeceAuthenticationOptions>()
            .Bind(configuration.GetSection(ImeceAuthenticationOptions.SectionName))
            .ValidateOnStart();

        services
            .AddOptions<DirectoryOptions>()
            .Bind(configuration.GetSection(DirectoryOptions.SectionName))
            .ValidateOnStart();

        // Configuration'da profil tanımlı değilse güvenli varsayılanlar uygulanır.
        services.PostConfigure<ImeceAuthenticationOptions>(options =>
        {
            if (options.Development.Profiles.Count == 0)
            {
                options.Development.Profiles =
                    DevelopmentAuthenticationDefaults.CreateAuthenticationProfiles();
            }
        });

        services.PostConfigure<DirectoryOptions>(options =>
        {
            if (options.Profiles.Count == 0)
            {
                options.Profiles =
                    DevelopmentAuthenticationDefaults.CreateDirectoryProfiles();
            }
        });

        var mode = configuration
            .GetSection(ImeceAuthenticationOptions.SectionName)
            .GetValue(nameof(ImeceAuthenticationOptions.Mode), AuthenticationMode.Development);

        // Startup validation: Development provider yalnızca Development ortamında.
        if (mode == AuthenticationMode.Development && !environment.IsDevelopment())
        {
            throw new InvalidOperationException(
                "Development authentication modu yalnızca Development ortamında "
                + "kullanılabilir. Production/Staging ortamında "
                + "Authentication:Mode değerini Negotiate veya EntraId olarak "
                + "ayarlayın.");
        }

        // Provider'dan bağımsız kimlik/authorization servisleri.
        services.AddHttpContextAccessor();
        services.AddScoped<IExternalIdentityAccessor, ClaimsExternalIdentityAccessor>();
        services.AddScoped<IApplicationUserResolver, ApplicationUserResolver>();
        services.AddScoped<ImeceUserContext>();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped<ICompanyContext, CompanyContext>();
        services.AddScoped<ICompanyAuthorizationService, CompanyAuthorizationService>();
        services.AddTransient<IClaimsTransformation, ImeceClaimsTransformation>();

        var directoryProvider = configuration
            .GetSection(DirectoryOptions.SectionName)
            .GetValue(nameof(DirectoryOptions.Provider), DirectoryProviderKind.Development);

        RegisterDirectoryProvider(services, mode, directoryProvider);
        RegisterAuthentication(services, mode);
        RegisterAuthorization(services);

        return services;
    }

    public static WebApplication UseImeceAuthentication(this WebApplication app)
    {
        app.UseAuthentication();

        // Doğrulanmış kimliği uygulama kullanıcısına çevirir (authorization öncesi).
        app.UseMiddleware<ImeceUserContextMiddleware>();

        return app;
    }

    private static void RegisterDirectoryProvider(
        IServiceCollection services,
        AuthenticationMode mode,
        DirectoryProviderKind directoryProvider)
    {
        // DB tabanlı dizin açıkça seçildiyse (Directory:Provider=Sql) provider'dan
        // bağımsız olarak SqlDirectoryUserService kullanılır. Sözleşme aynıdır;
        // AddImeceDatabase'in kayıtlı olması gerekir.
        if (directoryProvider == DirectoryProviderKind.Sql)
        {
            services.AddScoped<IDirectoryUserService, SqlDirectoryUserService>();
            return;
        }

        switch (mode)
        {
            case AuthenticationMode.Development:
                services.AddScoped<IDirectoryUserService, DevelopmentDirectoryUserService>();
                break;

            case AuthenticationMode.Negotiate:
            case AuthenticationMode.EntraId:
                // LDAP/AD dizin adapter'ı sınırı. Bu turda yapılandırılmadı;
                // kullanıldığında açık bir NotImplemented hatası verir.
                services.AddScoped<IDirectoryUserService, LdapDirectoryUserService>();
                break;
        }
    }

    private static void RegisterAuthentication(
        IServiceCollection services,
        AuthenticationMode mode)
    {
        switch (mode)
        {
            case AuthenticationMode.Development:
                services
                    .AddAuthentication(ImeceAuthenticationSchemes.Development)
                    .AddScheme<AuthenticationSchemeOptions, DevelopmentAuthenticationHandler>(
                        ImeceAuthenticationSchemes.Development,
                        _ => { });
                break;

            case AuthenticationMode.Negotiate:
                // Genişletme noktası: AddNegotiate + WindowsClaimsTransformer.
                throw new InvalidOperationException(
                    "Negotiate (Windows) authentication bu derlemede henüz "
                    + "yapılandırılmadı. Kurulum adımları için "
                    + "docs/authentication-development-to-windows-ad.md dosyasına bakın.");

            case AuthenticationMode.EntraId:
                // Genişletme noktası: Microsoft.Identity.Web / JWT bearer.
                throw new InvalidOperationException(
                    "Entra ID/JWT authentication bu derlemede henüz "
                    + "yapılandırılmadı.");
        }
    }

    private static void RegisterAuthorization(IServiceCollection services)
    {
        // Policy handler'ları ICurrentUser'a bağlı olduğundan scoped kaydedilir.
        services.AddScoped<IAuthorizationHandler, RegisteredUserAuthorizationHandler>();
        services.AddScoped<IAuthorizationHandler, CompanyAuthorizationHandler>();
        services.AddScoped<IAuthorizationHandler, RoleAuthorizationHandler>();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

        services.AddAuthorization(options =>
        {
            // Güvenlik ağı: açıkça [AllowAnonymous] işaretlenmemiş hiçbir endpoint
            // anonim kalmaz. Kimliği doğrulanmış kullanıcı zorunludur.
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();

            options.AddPolicy(
                ImecePolicies.RequireRegisteredUser,
                policy => policy.Requirements.Add(new RegisteredUserRequirement()));

            options.AddPolicy(
                ImecePolicies.RequireCompany,
                policy => policy.Requirements.Add(new CompanyRequirement()));

            options.AddPolicy(
                ImecePolicies.RequireCompanyAdmin,
                policy => policy.Requirements.Add(
                    new RoleRequirement(Roles.CompanyAdmin, Roles.GlobalAdmin)));

            options.AddPolicy(
                ImecePolicies.RequireGlobalAdmin,
                policy => policy.Requirements.Add(
                    new RoleRequirement(Roles.GlobalAdmin)));

            // Global (holding geneli) içerik yönetimi: content.global.manage izni
            // gerektirir. Sıradan CompanyAdmin bu izne sahip değildir.
            options.AddPolicy(
                ImecePolicies.RequireGlobalContentManager,
                policy => policy.Requirements.Add(
                    new PermissionRequirement(Permissions.ContentGlobalManage)));
        });
    }
}
