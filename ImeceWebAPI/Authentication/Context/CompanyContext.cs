using Application.Exceptions;
using Core.Authorization;

namespace ImeceWebAPI.Authentication.Context;

/// <summary>
/// <see cref="ICompanyContext"/>'in scoped implementasyonu.
/// <see cref="CurrentCompanyId"/> birincil üyelik içindir (audit /me gösterimi):
/// tek üyelikte o şirket, çoklu üyelikte null. Create/list filtreleri istek DTO'sundan gelir.
/// </summary>
public sealed class CompanyContext : ICompanyContext
{
    private readonly ImeceUserContext _context;

    public CompanyContext(ImeceUserContext context)
    {
        _context = context;
    }

    public int? CurrentCompanyId => ResolvePrimaryCompanyId();

    public int? CompanyId => CurrentCompanyId;

    public string? CompanyName
    {
        get
        {
            var companyId = CurrentCompanyId;
            if (companyId is null)
            {
                return null;
            }

            var membership = _context.User?.CompanyMemberships
                .FirstOrDefault(m => m.CompanyId == companyId);

            return membership?.CompanyName ?? _context.User?.CompanyName;
        }
    }

    public bool HasCompany => CurrentCompanyId.HasValue;

    public bool IsGlobalAdmin =>
        _context.User is { IsActive: true } user
        && user.Roles.Contains(Roles.GlobalAdmin, StringComparer.OrdinalIgnoreCase);

    public bool CanAccessCompany(int companyId)
    {
        if (IsGlobalAdmin)
        {
            return true;
        }

        return _context.User?.CompanyMemberships
            .Any(m => m.CompanyId == companyId) == true;
    }

    public void EnsureCanAccessCompany(int companyId)
    {
        if (!CanAccessCompany(companyId))
        {
            throw new ForbiddenException(
                "Bu şirkete ait veriye erişim yetkiniz bulunmuyor.");
        }
    }

    public int GetRequiredCompanyId() =>
        CurrentCompanyId
        ?? throw new ForbiddenException(
            "Bu işlem bir şirkete bağlı kullanıcı gerektirir.");

    private int? ResolvePrimaryCompanyId()
    {
        var user = _context.User;
        if (user is null)
        {
            return null;
        }

        var memberships = user.CompanyMemberships;
        if (memberships.Count == 1)
        {
            return memberships.First().CompanyId;
        }

        if (memberships.Count > 1)
        {
            return null;
        }

        return user.CompanyId;
    }
}
