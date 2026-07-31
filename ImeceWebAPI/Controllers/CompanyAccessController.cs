using Application.Services;
using Core.Authorization;
using ImeceWebAPI.Controllers.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ImeceWebAPI.Controllers;

/// <summary>
/// Current user için erişilebilir şirket lookup.
/// Global: canAccessAllCompanies=true ve tüm aktif şirketler.
/// Assigned: user_company_access kayıtları.
/// </summary>
[ApiController]
[Route("api/company-access/")]
[Authorize(Policy = ImecePolicies.RequireRegisteredUser)]
public sealed class CompanyAccessController : ApiControllerBase
{
    private readonly UserAccessService _userAccessService;

    public CompanyAccessController(UserAccessService userAccessService)
    {
        _userAccessService = userAccessService;
    }

    [HttpGet("accessible-companies")]
    public Task<IActionResult> GetAccessibleCompanies(CancellationToken cancellationToken) =>
        ExecuteAsync(
            _userAccessService.GetAccessibleCompaniesAsync,
            cancellationToken);
}
