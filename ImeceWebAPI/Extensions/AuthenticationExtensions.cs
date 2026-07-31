using System.Security.Claims;
using System.Text;
using Application.Services;
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
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

namespace ImeceWebAPI.Extensions;

/// <summary>
/// Authentication ve application authorization altyapısının kurulumu.
/// Provider seçimi configuration (<c>Authentication:Mode</c>) ile yapılır;
/// üst katmanlar (controller, service, repository) yalnızca
/// <see cref="ICurrentUser"/>/<see cref="ICompanyContext"/> soyutlamalarına
/// bağlıdır. Provider değişimi bu extension + configuration ile sınırlıdır.
///
/// Dev profile login root causes: (1) no login endpoint → LocalJwt + AuthController,
/// (2) FE GuestGuard stuck → real JWT Bearer, (3) SqlDirectory ExternalId mismatch
/// → seed azure_object_id aligned, (4) hardcoded permissions → role_permissions SQL.
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

        if (mode == AuthenticationMode.Development && !environment.IsDevelopment())
        {
            throw new InvalidOperationException(
                "Development authentication modu yalnızca Development ortamında "
                + "kullanılabilir. Production/Staging ortamında "
                + "Authentication:Mode değerini Negotiate, EntraId veya LocalJwt olarak "
                + "ayarlayın.");
        }

        services.AddHttpContextAccessor();
        services.AddScoped<IExternalIdentityAccessor, ClaimsExternalIdentityAccessor>();
        services.AddScoped<IApplicationUserResolver, ApplicationUserResolver>();
        services.AddScoped<ImeceUserContext>();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped<ICompanyContext, CompanyContext>();
        services.AddScoped<ICompanyAuthorizationService, CompanyAuthorizationService>();
        services.AddScoped<IAccessGuard, AccessGuard>();
        services.AddTransient<IClaimsTransformation, ImeceClaimsTransformation>();

        var directoryProvider = configuration
            .GetSection(DirectoryOptions.SectionName)
            .GetValue(nameof(DirectoryOptions.Provider), DirectoryProviderKind.Development);

        RegisterDirectoryProvider(services, mode, directoryProvider, environment);
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        RegisterAuthentication(services, mode, configuration, environment);
        RegisterAuthorization(services);

        return services;
    }

    public static WebApplication UseImeceAuthentication(this WebApplication app)
    {
        app.UseAuthentication();
        app.UseMiddleware<ImeceUserContextMiddleware>();
        return app;
    }

    private static void RegisterDirectoryProvider(
        IServiceCollection services,
        AuthenticationMode mode,
        DirectoryProviderKind directoryProvider,
        IHostEnvironment environment)
    {
        // LocalJwt + Development ortamı: admin JWT (SQL) ve web fake AD (development dizini).
        if (mode == AuthenticationMode.LocalJwt && environment.IsDevelopment())
        {
            services.AddScoped<DevelopmentDirectoryUserService>();
            services.AddScoped<SqlDirectoryUserService>();
            services.AddScoped<IDirectoryUserService, CompositeDirectoryUserService>();
            return;
        }

        // LocalJwt her zaman SQL dizin kullanır (ExternalId = azure_object_id).
        if (mode == AuthenticationMode.LocalJwt
            || directoryProvider == DirectoryProviderKind.Sql)
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
                services.AddScoped<IDirectoryUserService, LdapDirectoryUserService>();
                break;
        }
    }

    private static void RegisterAuthentication(
        IServiceCollection services,
        AuthenticationMode mode,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        switch (mode)
        {
            case AuthenticationMode.LocalJwt:
                if (environment.IsDevelopment())
                {
                    RegisterLocalJwtWithDevHeaderAuthentication(services, configuration);
                }
                else
                {
                    RegisterLocalJwtAuthentication(services, configuration);
                }

                break;

            case AuthenticationMode.Development:
                services
                    .AddAuthentication(ImeceAuthenticationSchemes.Development)
                    .AddScheme<AuthenticationSchemeOptions, DevelopmentAuthenticationHandler>(
                        ImeceAuthenticationSchemes.Development,
                        _ => { });
                break;

            case AuthenticationMode.Negotiate:
                throw new InvalidOperationException(
                    "Negotiate (Windows) authentication bu derlemede henüz "
                    + "yapılandırılmadı. Kurulum adımları için "
                    + "docs/authentication-development-to-windows-ad.md dosyasına bakın.");

            case AuthenticationMode.EntraId:
                throw new InvalidOperationException(
                    "Entra ID/JWT authentication bu derlemede henüz "
                    + "yapılandırılmadı.");
        }
    }

    private static void RegisterLocalJwtWithDevHeaderAuthentication(
        IServiceCollection services,
        IConfiguration configuration)
    {
        var devHeaderName = configuration
            .GetSection($"{ImeceAuthenticationOptions.SectionName}:Development")
            .GetValue<string>(nameof(DevelopmentAuthenticationSettings.UserHeaderName))
            ?? "X-Imece-Dev-User";

        var authBuilder = services
            .AddAuthentication(ImeceAuthenticationSchemes.DevComposite)
            .AddPolicyScheme(
                ImeceAuthenticationSchemes.DevComposite,
                "Development LocalJwt + fake Azure AD",
                options =>
                {
                    options.ForwardDefaultSelector = context =>
                    {
                        if (context.Request.Headers.TryGetValue(devHeaderName, out var devUser)
                            && !string.IsNullOrWhiteSpace(devUser))
                        {
                            return ImeceAuthenticationSchemes.Development;
                        }

                        return JwtBearerDefaults.AuthenticationScheme;
                    };
                });

        ConfigureJwtBearer(authBuilder, configuration);

        authBuilder.AddScheme<AuthenticationSchemeOptions, DevelopmentAuthenticationHandler>(
            ImeceAuthenticationSchemes.Development,
            _ => { });
    }

    private static void RegisterLocalJwtAuthentication(
        IServiceCollection services,
        IConfiguration configuration)
    {
        ConfigureJwtBearer(
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme),
            configuration);
    }

    private static AuthenticationBuilder ConfigureJwtBearer(
        AuthenticationBuilder authBuilder,
        IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection(
            $"{ImeceAuthenticationOptions.SectionName}:Jwt");

        var issuer = jwtSection.GetValue<string>(nameof(JwtAuthenticationSettings.Issuer))
            ?? "imece-webapi";
        var audience = jwtSection.GetValue<string>(nameof(JwtAuthenticationSettings.Audience))
            ?? "imece-admin";
        var signingKey = jwtSection.GetValue<string>(nameof(JwtAuthenticationSettings.SigningKey))
            ?? string.Empty;

        if (string.IsNullOrWhiteSpace(signingKey))
        {
            throw new InvalidOperationException(
                "LocalJwt modu için Authentication:Jwt:SigningKey yapılandırılmalıdır.");
        }

        return authBuilder.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = issuer,
                ValidAudience = audience,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(signingKey)),
                NameClaimType = ImeceClaimTypes.Username,
                RoleClaimType = ClaimTypes.Role
            };

            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = context =>
                {
                    var principal = context.Principal;
                    if (principal?.Identity is not { IsAuthenticated: true })
                    {
                        return Task.CompletedTask;
                    }

                    var identity = (ClaimsIdentity)principal.Identity;

                    if (!principal.HasClaim(c => c.Type == ImeceClaimTypes.ExternalId))
                    {
                        var sub = principal.FindFirst("sub")?.Value;
                        if (!string.IsNullOrWhiteSpace(sub))
                        {
                            identity.AddClaim(new Claim(ImeceClaimTypes.ExternalId, sub));
                        }
                    }

                    if (!principal.HasClaim(c => c.Type == ImeceClaimTypes.IdentityProvider))
                    {
                        identity.AddClaim(new Claim(
                            ImeceClaimTypes.IdentityProvider,
                            ImeceIdentityProviders.Local));
                    }

                    return Task.CompletedTask;
                }
            };
        });
    }

    private static void RegisterAuthorization(IServiceCollection services)
    {
        services.AddScoped<IAuthorizationHandler, RegisteredUserAuthorizationHandler>();
        services.AddScoped<IAuthorizationHandler, CompanyAuthorizationHandler>();
        services.AddScoped<IAuthorizationHandler, RoleAuthorizationHandler>();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

        services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();

            options.AddPolicy(
                ImecePolicies.RequireRegisteredUser,
                policy => policy.Requirements.Add(new RegisteredUserRequirement()));

            options.AddPolicy(
                ImecePolicies.RequireCompany,
                policy => policy.Requirements.Add(new CompanyRequirement()));

            // Module permission policies. manage-implies-view is enforced in
            // PermissionSatisfaction / PermissionAuthorizationHandler, so view
            // policies only need the view code.
            void AddPermissionPolicy(string name, params string[] permissions) =>
                options.AddPolicy(
                    name,
                    policy => policy.Requirements.Add(new PermissionRequirement(permissions)));

            AddPermissionPolicy(ImecePolicies.RequireAdminPanelAccess, Permissions.AdminPanelAccess);

            AddPermissionPolicy(ImecePolicies.RequireUsersView, Permissions.UsersView);
            AddPermissionPolicy(ImecePolicies.RequireUsersManage, Permissions.UsersManage);

            AddPermissionPolicy(ImecePolicies.RequireRolesView, Permissions.RolesView);
            AddPermissionPolicy(ImecePolicies.RequireRolesManage, Permissions.RolesManage);

            AddPermissionPolicy(ImecePolicies.RequirePermissionsView, Permissions.PermissionsView);
            AddPermissionPolicy(ImecePolicies.RequirePermissionsManage, Permissions.PermissionsManage);

            AddPermissionPolicy(ImecePolicies.RequireOrganizationView, Permissions.OrganizationView);
            AddPermissionPolicy(ImecePolicies.RequireOrganizationManage, Permissions.OrganizationManage);

            AddPermissionPolicy(ImecePolicies.RequireContentView, Permissions.ContentView);
            AddPermissionPolicy(
                ImecePolicies.RequireContentCompanyManage,
                Permissions.ContentCompanyManage,
                Permissions.ContentGlobalManage);
            AddPermissionPolicy(ImecePolicies.RequireContentGlobalManage, Permissions.ContentGlobalManage);

            AddPermissionPolicy(ImecePolicies.RequireMediaView, Permissions.MediaView);
            AddPermissionPolicy(ImecePolicies.RequireMediaManage, Permissions.MediaManage);

            AddPermissionPolicy(ImecePolicies.RequireMenusView, Permissions.MenusView);
            AddPermissionPolicy(ImecePolicies.RequireMenusManage, Permissions.MenusManage);

            AddPermissionPolicy(ImecePolicies.RequireServicesView, Permissions.ServicesView);
            AddPermissionPolicy(ImecePolicies.RequireServicesManage, Permissions.ServicesManage);

            AddPermissionPolicy(ImecePolicies.RequireReservationsView, Permissions.ReservationsView);
            AddPermissionPolicy(ImecePolicies.RequireReservationsManage, Permissions.ReservationsManage);

            AddPermissionPolicy(ImecePolicies.RequireReportsView, Permissions.ReportsView);

#pragma warning disable CS0618 // Legacy policy name aliases map to permission policies
            AddPermissionPolicy(
                ImecePolicies.RequireCompanyAdmin,
                Permissions.ContentCompanyManage,
                Permissions.OrganizationManage,
                Permissions.UsersManage);
            AddPermissionPolicy(
                ImecePolicies.RequireGlobalAdmin,
                Permissions.OrganizationManage,
                Permissions.PermissionsManage);
            AddPermissionPolicy(
                ImecePolicies.RequireGlobalContentManager,
                Permissions.ContentGlobalManage);
            AddPermissionPolicy(
                ImecePolicies.RequireCompanyAdminOrGlobalContentManager,
                Permissions.ContentCompanyManage,
                Permissions.ContentGlobalManage);
#pragma warning restore CS0618
        });
    }
}
