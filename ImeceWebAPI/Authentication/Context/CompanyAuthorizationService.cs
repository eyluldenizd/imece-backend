using Application.Exceptions;
using Core.Authorization;

namespace ImeceWebAPI.Authentication.Context;

/// <summary>
/// <see cref="ICompanyAuthorizationService"/>'in scoped implementasyonu.
/// Global organization access yalnız şirket kapsamını açar; permission'lar rol birleşiminden gelir.
/// </summary>
public sealed class CompanyAuthorizationService : ICompanyAuthorizationService
{
    private readonly ImeceUserContext _context;

    public CompanyAuthorizationService(ImeceUserContext context)
    {
        _context = context;
    }

    private ApplicationUser? User => _context.User;

    public bool IsGlobalAdmin =>
        User is { IsActive: true, HasGlobalOrganizationAccess: true };

    public bool CanAccessAllCompanies => IsGlobalAdmin;

    public IReadOnlyCollection<int> GetAccessibleCompanyIds() =>
        User?.CompanyMemberships
            .Select(membership => membership.CompanyId)
            .Distinct()
            .ToArray()
        ?? [];

    public bool CanAccessCompany(int companyId)
    {
        if (CanAccessAllCompanies)
        {
            return true;
        }

        return User?.CompanyMemberships.Any(
            membership => membership.CompanyId == companyId) ?? false;
    }

    public bool HasPermission(int companyId, string permission)
    {
        if (User is not { IsActive: true } user)
        {
            return false;
        }

        if (!CanAccessCompany(companyId))
        {
            return false;
        }

        return PermissionSatisfaction.Satisfies(user.Permissions, permission);
    }

    public void EnsurePermission(int companyId, string permission)
    {
        if (!HasPermission(companyId, permission))
        {
            throw new ForbiddenException(
                "Bu şirkette bu işlemi yapma yetkiniz bulunmuyor.");
        }
    }
}
