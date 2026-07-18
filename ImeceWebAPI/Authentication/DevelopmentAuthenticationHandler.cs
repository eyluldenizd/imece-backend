using System.Text.Encodings.Web;
using Core.Authentication;
using ImeceWebAPI.Authentication.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace ImeceWebAPI.Authentication;

/// <summary>
/// Development/test amaçlı, gerçek bir kimlik sağlayıcıyı (ör. AD) taklit eden
/// authentication handler. Yalnızca Development ortamında kaydedilir; header
/// veya configuration ile farklı kullanıcı senaryoları seçilebilir. Ürettiği
/// normalize edilmiş Imece claim'leri sayesinde üst katmanlar bu handler'a
/// bağımlı olmaz.
/// </summary>
public sealed class DevelopmentAuthenticationHandler
    : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IOptionsMonitor<ImeceAuthenticationOptions> _imeceOptions;
    private readonly IHostEnvironment _environment;

    public DevelopmentAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IOptionsMonitor<ImeceAuthenticationOptions> imeceOptions,
        IHostEnvironment environment)
        : base(options, logger, encoder)
    {
        _imeceOptions = imeceOptions;
        _environment = environment;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var settings = _imeceOptions.CurrentValue.Development;
        var profileKey = settings.DefaultProfile;

        if (settings.AllowHeaderSelection
            && _environment.IsDevelopment()
            && Request.Headers.TryGetValue(settings.UserHeaderName, out var header)
            && !string.IsNullOrWhiteSpace(header))
        {
            profileKey = header.ToString();
        }

        if (!settings.Profiles.TryGetValue(profileKey, out var profile))
        {
            return Task.FromResult(AuthenticateResult.Fail(
                $"Bilinmeyen development kullanıcı profili: '{profileKey}'."));
        }

        var principal = ImeceClaimsPrincipalFactory.Create(
            Scheme.Name,
            profile.IdentityProvider ?? ImeceIdentityProviders.Development,
            profile.ExternalId,
            profile.DomainOrTenant,
            profile.Username,
            profile.Email,
            profile.DisplayName);

        var ticket = new AuthenticationTicket(principal, Scheme.Name);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
