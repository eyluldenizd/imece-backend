using System.Security.Claims;
using Core.Authentication;
using Microsoft.AspNetCore.Authentication;

namespace ImeceWebAPI.Authentication;

/// <summary>
/// Provider'dan gelen principal'ı Imece standart claim setine normalize etmek
/// için merkezî genişletme noktası. Development ve Test handler'ları claim'leri
/// zaten normalize üretir; bu durumda dönüşüm idempotenttir.
///
/// Windows/Negotiate veya Entra ID eklendiğinde, provider'a özgü claim'leri
/// (Windows SID/UPN, JWT "oid"/"preferred_username" vb.) Imece claim'lerine
/// çeviren mantık burada uygulanır; üst katmanlar değişmez.
/// </summary>
public sealed class ImeceClaimsTransformation : IClaimsTransformation
{
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        // Zaten normalize edilmişse (Imece external_id mevcutsa) hiçbir şey yapma.
        if (principal.Identity is not { IsAuthenticated: true }
            || principal.HasClaim(claim => claim.Type == ImeceClaimTypes.ExternalId))
        {
            return Task.FromResult(principal);
        }

        // Genişletme noktası: provider'a özgü claim'ler burada Imece claim'lerine
        // dönüştürülür. Bu turda yalnızca normalize üreten provider'lar aktif
        // olduğundan ek dönüşüm yapılmaz.
        return Task.FromResult(principal);
    }
}
