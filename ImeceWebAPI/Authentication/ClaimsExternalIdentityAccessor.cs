using System.Security.Claims;
using Core.Authentication;

namespace ImeceWebAPI.Authentication;

/// <summary>
/// Mevcut isteğin <see cref="ClaimsPrincipal"/>'ından normalize edilmiş Imece
/// claim'lerini okuyarak <see cref="ExternalIdentity"/> üretir. Provider'a
/// özgü claim tiplerine bağlanmaz; yalnızca standart Imece claim'lerini kullanır.
/// </summary>
public sealed class ClaimsExternalIdentityAccessor : IExternalIdentityAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ClaimsExternalIdentityAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public ExternalIdentity? GetExternalIdentity()
    {
        var principal = _httpContextAccessor.HttpContext?.User;

        if (principal?.Identity is not { IsAuthenticated: true })
        {
            return null;
        }

        var externalId = principal.FindFirstValue(ImeceClaimTypes.ExternalId);
        var identityProvider = principal.FindFirstValue(ImeceClaimTypes.IdentityProvider);

        if (string.IsNullOrWhiteSpace(externalId)
            || string.IsNullOrWhiteSpace(identityProvider))
        {
            return null;
        }

        return new ExternalIdentity
        {
            IdentityProvider = identityProvider,
            ExternalId = externalId,
            DomainOrTenant = principal.FindFirstValue(ImeceClaimTypes.DomainOrTenant),
            Username = principal.FindFirstValue(ImeceClaimTypes.Username),
            Email = principal.FindFirstValue(ImeceClaimTypes.Email),
            DisplayName = principal.FindFirstValue(ImeceClaimTypes.DisplayName)
        };
    }
}
