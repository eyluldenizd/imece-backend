namespace Core.Authorization;

/// <summary>
/// Merkezi permission + company scope doğrulama. Request-scoped cache ile
/// aynı request içinde tekrarlı DB sorgusu yapmaz; kaynak <see cref="ICurrentUser"/>.
/// </summary>
public interface IAccessGuard
{
    OrganizationAccessScope OrganizationScope { get; }

    bool HasGlobalOrganizationAccess { get; }

    IReadOnlyCollection<int> AccessibleCompanyIds { get; }

    bool HasPermission(string permissionCode);

    bool HasAnyPermission(params string[] permissionCodes);

    void RequirePermission(string permissionCode);

    void RequireAnyPermission(params string[] permissionCodes);

    bool CanAccessCompany(int companyId);

    void RequireCompanyAccess(int companyId);
}
