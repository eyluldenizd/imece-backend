using System.Security.Claims;
using Core.Authentication;

namespace ImeceWebAPI.Authentication;

/// <summary>
/// Normalize edilmiş Imece claim'lerinden bir <see cref="ClaimsPrincipal"/>
/// üretir. Tüm provider'lar (Development, Test ve ileride Windows/Entra
/// transformer'ları) aynı standart claim setini üretmek için bu fabrikayı
/// kullanır.
/// </summary>
public static class ImeceClaimsPrincipalFactory
{
    public static ClaimsPrincipal Create(
        string authenticationScheme,
        string identityProvider,
        string externalId,
        string? domainOrTenant,
        string? username,
        string? email,
        string? displayName)
    {
        var claims = new List<Claim>
        {
            new(ImeceClaimTypes.IdentityProvider, identityProvider),
            new(ImeceClaimTypes.ExternalId, externalId)
        };

        AddIfPresent(claims, ImeceClaimTypes.DomainOrTenant, domainOrTenant);
        AddIfPresent(claims, ImeceClaimTypes.Username, username);
        AddIfPresent(claims, ImeceClaimTypes.Email, email);
        AddIfPresent(claims, ImeceClaimTypes.DisplayName, displayName);

        // Framework kolaylığı için standart Name/NameIdentifier claim'leri.
        claims.Add(new Claim(ClaimTypes.NameIdentifier, externalId));
        if (!string.IsNullOrWhiteSpace(username))
        {
            claims.Add(new Claim(ClaimTypes.Name, username));
        }

        var identity = new ClaimsIdentity(
            claims,
            authenticationScheme,
            ImeceClaimTypes.Username,
            ClaimTypes.Role);

        return new ClaimsPrincipal(identity);
    }

    private static void AddIfPresent(
        ICollection<Claim> claims,
        string type,
        string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            claims.Add(new Claim(type, value));
        }
    }
}
