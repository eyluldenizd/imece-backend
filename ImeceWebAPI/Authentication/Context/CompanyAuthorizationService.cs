using Application.Exceptions;
using Core.Authorization;

namespace ImeceWebAPI.Authentication.Context;

/// <summary>
/// <see cref="ICompanyAuthorizationService"/>'in scoped implementasyonu ve
/// çoklu şirket yetki kararlarının tek merkezi. Kararlar çözümlenmiş
/// <see cref="ApplicationUser"/> üzerinden verilir; authentication
/// provider'dan bağımsızdır.
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
        User is { IsActive: true } user
        && user.Roles.Contains(Roles.GlobalAdmin, StringComparer.OrdinalIgnoreCase);

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

        // Global admin tüm şirketlerde tüm izinlere sahiptir (merkezi kural).
        if (IsGlobalAdmin)
        {
            return true;
        }

        var membership = user.CompanyMemberships.FirstOrDefault(
            m => m.CompanyId == companyId);

        return membership is not null
            && membership.Permissions.Contains(
                permission,
                StringComparer.OrdinalIgnoreCase);
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
