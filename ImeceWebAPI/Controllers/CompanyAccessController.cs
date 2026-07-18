using Core.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ImeceWebAPI.Controllers;

/// <summary>
/// Kullanıcının erişebildiği şirketleri döndürür. Admin panel, çoklu şirket
/// yetkili bir kullanıcı için hedef şirket seçimini bu listeye göre yapar.
/// Yetki kaynağı frontend değil; backend'deki üyelik/rol kayıtlarıdır.
/// </summary>
[ApiController]
[Route("api/company-access/")]
[Authorize(Policy = ImecePolicies.RequireRegisteredUser)]
public sealed class CompanyAccessController : ControllerBase
{
    private readonly ICurrentUser _currentUser;
    private readonly ICompanyAuthorizationService _companyAuthorization;

    public CompanyAccessController(
        ICurrentUser currentUser,
        ICompanyAuthorizationService companyAuthorization)
    {
        _currentUser = currentUser;
        _companyAuthorization = companyAuthorization;
    }

    [HttpGet("accessible-companies")]
    public IActionResult GetAccessibleCompanies() => Ok(new
    {
        // Global admin tüm şirketlere erişebilir; tam liste Companies
        // kaynağından çözülür (bu bayrak frontend'e bunu bildirir).
        canAccessAllCompanies = _companyAuthorization.CanAccessAllCompanies,
        companies = _currentUser.CompanyMemberships
            .Select(membership => new
            {
                companyId = membership.CompanyId,
                companyName = membership.CompanyName,
                roles = membership.Roles
            })
    });
}
