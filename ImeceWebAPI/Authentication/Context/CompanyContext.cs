using Application.Exceptions;
using Core.Authorization;

namespace ImeceWebAPI.Authentication.Context;

/// <summary>
/// <see cref="ICompanyContext"/>'in scoped implementasyonu ve şirket erişim
/// kararlarının tek merkezi. Şirket bilgisi çözümlenmiş uygulama
/// kullanıcısından gelir; authentication provider'dan bağımsızdır.
/// </summary>
public sealed class CompanyContext : ICompanyContext
{
    private readonly ImeceUserContext _context;

    public CompanyContext(ImeceUserContext context)
    {
        _context = context;
    }

    public int? CurrentCompanyId => _context.User?.CompanyId;

    public int? CompanyId => CurrentCompanyId;

    public string? CompanyName => _context.User?.CompanyName;

    public bool HasCompany => _context.User?.HasCompany ?? false;

    public bool IsGlobalAdmin =>
        _context.User is { IsActive: true } user
        && user.Roles.Contains(Roles.GlobalAdmin, StringComparer.OrdinalIgnoreCase);

    public bool CanAccessCompany(int companyId)
    {
        // Global admin dışında cross-company erişim reddedilir (tek kural).
        if (IsGlobalAdmin)
        {
            return true;
        }

        return HasCompany && CurrentCompanyId == companyId;
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
}
