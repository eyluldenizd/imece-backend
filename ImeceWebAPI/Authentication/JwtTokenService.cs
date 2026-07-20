using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.Services;
using Core.Authentication;
using Core.Authorization;
using ImeceWebAPI.Authentication.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ImeceWebAPI.Authentication;

/// <summary>
/// Stateless JWT access token üretimi. imece:* claim'leri yazılır;
/// <see cref="ClaimsExternalIdentityAccessor"/> JWT principal'dan okur.
/// </summary>
public sealed class JwtTokenService : IJwtTokenService
{
    private readonly IOptionsMonitor<ImeceAuthenticationOptions> _options;

    public JwtTokenService(IOptionsMonitor<ImeceAuthenticationOptions> options)
    {
        _options = options;
    }

    public (string AccessToken, DateTime ExpiresAt) CreateAccessToken(ApplicationUser user)
    {
        var jwtOptions = _options.CurrentValue.Jwt;
        var signingKey = jwtOptions.SigningKey;

        if (string.IsNullOrWhiteSpace(signingKey))
        {
            throw new InvalidOperationException(
                "Authentication:Jwt:SigningKey yapılandırılmamış.");
        }

        var expiresAt = DateTime.UtcNow.AddMinutes(jwtOptions.AccessTokenMinutes);
        var identity = user.Identity;

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.UserId!.Value.ToString()),
            new(ImeceClaimTypes.IdentityProvider, ImeceIdentityProviders.Local),
            new(ImeceClaimTypes.ExternalId, identity.ExternalId),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
        };

        AddIfPresent(claims, ImeceClaimTypes.Username, identity.Username);
        AddIfPresent(claims, ImeceClaimTypes.Email, identity.Email);
        AddIfPresent(claims, ImeceClaimTypes.DisplayName, identity.DisplayName);
        AddIfPresent(claims, ImeceClaimTypes.DomainOrTenant, identity.DomainOrTenant);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtOptions.Issuer,
            audience: jwtOptions.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresAt,
            signingCredentials: credentials);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
        return (accessToken, expiresAt);
    }

    private static void AddIfPresent(ICollection<Claim> claims, string type, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            claims.Add(new Claim(type, value));
        }
    }
}
