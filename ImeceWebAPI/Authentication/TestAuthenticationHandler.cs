using System.Text.Encodings.Web;
using Core.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace ImeceWebAPI.Authentication;

/// <summary>
/// Otomatik testler için, dış kimliği request header'larından okuyan
/// authentication handler. Development handler'dan farklı bir "provider" gibi
/// davranır ancak aynı normalize edilmiş Imece claim'lerini üretir; böylece
/// provider değişiminin üst katman davranışını değiştirmediği doğrulanabilir.
/// </summary>
public sealed class TestAuthenticationHandler
    : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string ExternalIdHeader = "X-Test-ExternalId";
    public const string ProviderHeader = "X-Test-Provider";
    public const string UsernameHeader = "X-Test-Username";
    public const string DomainHeader = "X-Test-Domain";

    public TestAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(ExternalIdHeader, out var externalId)
            || string.IsNullOrWhiteSpace(externalId))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var provider = Request.Headers.TryGetValue(ProviderHeader, out var p)
            && !string.IsNullOrWhiteSpace(p)
            ? p.ToString()
            : ImeceIdentityProviders.Test;

        var username = Request.Headers.TryGetValue(UsernameHeader, out var u)
            ? u.ToString()
            : null;

        var domain = Request.Headers.TryGetValue(DomainHeader, out var d)
            ? d.ToString()
            : null;

        var principal = ImeceClaimsPrincipalFactory.Create(
            Scheme.Name,
            provider,
            externalId.ToString(),
            domain,
            username,
            email: null,
            displayName: username);

        var ticket = new AuthenticationTicket(principal, Scheme.Name);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
