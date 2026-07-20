using Application.DTOs;
using Application.Exceptions;
using Core.Authorization;

namespace Application.Services;

public interface IJwtTokenService
{
    (string AccessToken, DateTime ExpiresAt) CreateAccessToken(ApplicationUser user);
}

public interface IAuthenticationService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);
}

public sealed class AuthenticationService : IAuthenticationService
{
    public Task<LoginResponseDto> LoginAsync(
        LoginRequestDto request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        throw new UnauthorizedAppException(
            "Local JWT için güvenli bir parola doğrulama kaynağı yapılandırılmamış.");
    }
}
