using Core.Authentication;
using Core.Authorization;
using Core.Directory;

namespace Infrastructure.Authentication.Directory;

/// <summary>
/// LDAP/Active Directory dizin adapter'ı için yer tutucu. Bu derlemede
/// yapılandırılmamıştır; Negotiate/EntraId + LDAP kurulumu ayrı bir turdur.
/// </summary>
public sealed class LdapDirectoryUserService : IDirectoryUserService
{
    public Task<ApplicationUser?> FindByExternalIdentityAsync(
        ExternalIdentity identity,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException(
            "LDAP directory provider is not configured in this build. "
            + "Set Directory:Provider=Sql for database-backed directory lookup, "
            + "or Authentication:Mode=Development with Directory:Provider=Development "
            + "for local profiles.");
    }
}
