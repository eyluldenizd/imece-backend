using Core.Authentication;
using Core.Authorization;
using Core.Directory;

namespace Infrastructure.Authentication.Directory;

/// <summary>
/// Development ortamında LocalJwt + fake Azure AD birlikte çalışsın diye
/// kimlik sağlayıcısına göre SQL veya development dizinine yönlendirir.
/// </summary>
public sealed class CompositeDirectoryUserService : IDirectoryUserService
{
    private readonly DevelopmentDirectoryUserService _development;
    private readonly SqlDirectoryUserService _sql;

    public CompositeDirectoryUserService(
        DevelopmentDirectoryUserService development,
        SqlDirectoryUserService sql)
    {
        _development = development;
        _sql = sql;
    }

    public Task<ApplicationUser?> FindByExternalIdentityAsync(
        ExternalIdentity identity,
        CancellationToken cancellationToken = default)
    {
        if (string.Equals(
                identity.IdentityProvider,
                ImeceIdentityProviders.Development,
                StringComparison.OrdinalIgnoreCase))
        {
            return _development.FindByExternalIdentityAsync(identity, cancellationToken);
        }

        return _sql.FindByExternalIdentityAsync(identity, cancellationToken);
    }
}
