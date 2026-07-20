using Application.DTOs;

namespace Application.Services;

public interface IAuthenticationService
{
    Task<LoginResponseDto> LoginAsync(
        LoginRequestDto request,
        CancellationToken cancellationToken = default);
}
