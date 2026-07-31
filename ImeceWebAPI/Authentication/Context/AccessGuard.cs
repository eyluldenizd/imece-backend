using Application.Exceptions;
using Core.Authorization;

namespace ImeceWebAPI.Authentication.Context;

/// <summary>
/// Request-scoped access guard. Kaynak <see cref="ICurrentUser"/> /
/// <see cref="ICompanyAuthorizationService"/>; ekstra DB çağrısı yapmaz.
/// Global organization access yalnız şirket kapsamını etkiler — tüm permission'ları vermez.
/// </summary>
public sealed class AccessGuard : IAccessGuard
{
    private readonly ICurrentUser _currentUser;
    private readonly ICompanyAuthorizationService _companyAuthorization;

    public AccessGuard(
        ICurrentUser currentUser,
        ICompanyAuthorizationService companyAuthorization)
    {
        _currentUser = currentUser;
        _companyAuthorization = companyAuthorization;
    }

    public OrganizationAccessScope OrganizationScope =>
        OrganizationScopeCodes.FromHasGlobalAccess(HasGlobalOrganizationAccess);

    public bool HasGlobalOrganizationAccess => _companyAuthorization.CanAccessAllCompanies;

    public IReadOnlyCollection<int> AccessibleCompanyIds =>
        _companyAuthorization.GetAccessibleCompanyIds();

    public bool HasPermission(string permissionCode)
    {
        if (_currentUser is not { IsActive: true })
        {
            return false;
        }

        return PermissionSatisfaction.Satisfies(_currentUser.Permissions, permissionCode);
    }

    public bool HasAnyPermission(params string[] permissionCodes) =>
        PermissionSatisfaction.SatisfiesAny(_currentUser.Permissions, permissionCodes);

    public void RequirePermission(string permissionCode)
    {
        if (!HasPermission(permissionCode))
        {
            throw new ForbiddenException(
                $"Bu işlem için '{permissionCode}' yetkisi gereklidir.");
        }
    }

    public void RequireAnyPermission(params string[] permissionCodes)
    {
        if (!HasAnyPermission(permissionCodes))
        {
            throw new ForbiddenException(
                "Bu işlem için gerekli yetkiye sahip değilsiniz.");
        }
    }

    public bool CanAccessCompany(int companyId) =>
        _companyAuthorization.CanAccessCompany(companyId);

    public void RequireCompanyAccess(int companyId)
    {
        if (!CanAccessCompany(companyId))
        {
            throw new ForbiddenException(
                "Bu şirkete ait veriye erişim yetkiniz bulunmuyor.");
        }
    }
}
