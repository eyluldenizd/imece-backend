using Core.Authentication;
using Core.Authorization;

namespace Core.Directory;

public enum DirectoryProviderKind
{
    Development = 0,
    Sql = 1,
    Ldap = 2
}

public sealed class DirectoryOptions
{
    public const string SectionName = "Directory";

    public DirectoryProviderKind Provider { get; set; } = DirectoryProviderKind.Development;

    public Dictionary<string, DirectoryProfileOptions> Profiles { get; set; } =
        new(StringComparer.OrdinalIgnoreCase);
}

public sealed class DirectoryProfileOptions
{
    public int UserId { get; set; }

    public string? Username { get; set; }

    public bool IsActive { get; set; } = true;

    public int? CompanyId { get; set; }

    public string? CompanyName { get; set; }

    public List<string> Roles { get; set; } = [];

    public List<string> Permissions { get; set; } = [];

    public List<DirectoryCompanyRoleOptions> CompanyRoles { get; set; } = [];
}

public sealed class DirectoryCompanyRoleOptions
{
    public int CompanyId { get; set; }

    public string? CompanyName { get; set; }

    public List<string> Roles { get; set; } = [];

    public List<string> Permissions { get; set; } = [];
}

public interface IDirectoryUserService
{
    Task<ApplicationUser?> FindByExternalIdentityAsync(
        ExternalIdentity identity,
        CancellationToken cancellationToken = default);
}
