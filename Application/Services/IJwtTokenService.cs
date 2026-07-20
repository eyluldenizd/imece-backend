using Core.Authorization;

namespace Application.Services;

public interface IJwtTokenService
{
    (string AccessToken, DateTime ExpiresAt) CreateAccessToken(ApplicationUser user);
}
