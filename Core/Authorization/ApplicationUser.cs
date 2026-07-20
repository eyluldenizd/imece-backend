using Core.Authentication;

namespace Core.Authorization;

public sealed class CompanyMembership
{
    public required int CompanyId { get; init; }

    public string? CompanyName { get; init; }

    public IReadOnlyCollection<string> Roles { get; init; } = [];

    public IReadOnlyCollection<string> Permissions { get; init; } = [];
}

public sealed class ApplicationUser
{
    public ExternalIdentity Identity { get; init; } = null!;

    public int? UserId { get; init; }

    public bool IsRegistered => UserId.HasValue;

    public bool IsActive { get; init; } = true;

    public int? CompanyId { get; init; }

    public string? CompanyName { get; init; }

    public bool HasCompany => CompanyId.HasValue;

    public IReadOnlyCollection<string> Roles { get; init; } = [];

    public IReadOnlyCollection<string> Permissions { get; init; } = [];

    public IReadOnlyCollection<CompanyMembership> CompanyMemberships { get; init; } = [];
}

public interface ICurrentUser
{
    bool IsAuthenticated { get; }

    bool IsRegistered { get; }

    bool IsActive { get; }

    int? UserId { get; }

    string? ExternalId { get; }

    string? Username { get; }

    string? Email { get; }

    string? DisplayName { get; }

    string? IdentityProvider { get; }

    IReadOnlyCollection<string> Roles { get; }

    IReadOnlyCollection<string> Permissions { get; }

    IReadOnlyCollection<CompanyMembership> CompanyMemberships { get; }

    bool IsInRole(string role);

    bool HasPermission(string permission);

    int GetRequiredUserId();
}

public interface ICompanyContext
{
    int? CurrentCompanyId { get; }

    int? CompanyId { get; }

    string? CompanyName { get; }

    bool HasCompany { get; }

    bool IsGlobalAdmin { get; }

    bool CanAccessCompany(int companyId);

    void EnsureCanAccessCompany(int companyId);

    int GetRequiredCompanyId();
}

public interface ICompanyAuthorizationService
{
    bool IsGlobalAdmin { get; }

    bool CanAccessAllCompanies { get; }

    IReadOnlyCollection<int> GetAccessibleCompanyIds();

    bool CanAccessCompany(int companyId);

    bool HasPermission(int companyId, string permission);

    void EnsurePermission(int companyId, string permission);
}

public interface IApplicationUserResolver
{
    Task<ApplicationUser> ResolveAsync(
        ExternalIdentity identity,
        CancellationToken cancellationToken = default);
}
