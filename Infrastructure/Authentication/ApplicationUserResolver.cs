using Core.Authentication;
using Core.Authorization;
using Core.Directory;

namespace Infrastructure.Authentication;

/// <summary>
/// Dış kimliği (<see cref="ExternalIdentity"/>) uygulama kullanıcısına çevirir.
/// Dizin kaydı yoksa kimlik doğrulanmış ama kayıtsız (AD-only) kullanıcı döner.
/// </summary>
public sealed class ApplicationUserResolver : IApplicationUserResolver
{
    private readonly IDirectoryUserService _directoryUserService;

    public ApplicationUserResolver(IDirectoryUserService directoryUserService)
    {
        _directoryUserService = directoryUserService;
    }

    public async Task<ApplicationUser> ResolveAsync(
        ExternalIdentity identity,
        CancellationToken cancellationToken = default)
    {
        var directoryUser = await _directoryUserService.FindByExternalIdentityAsync(
            identity,
            cancellationToken);

        if (directoryUser is not null)
        {
            return directoryUser;
        }

        return new ApplicationUser
        {
            Identity = identity,
            UserId = null,
            IsActive = true,
            Roles = [],
            Permissions = [],
            CompanyMemberships = []
        };
    }
}
