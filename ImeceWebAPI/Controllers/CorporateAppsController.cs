using Application.Services;
using Core.Authorization;
using ImeceWebAPI.Controllers.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ImeceWebAPI.Controllers;

[ApiController]
[Route("api/corporate-apps/")]
[Authorize(Policy = ImecePolicies.RequireRegisteredUser)]
public sealed class CorporateAppsController : ApiControllerBase
{
    private readonly CorporateAppService _corporateAppService;

    public CorporateAppsController(CorporateAppService corporateAppService)
    {
        _corporateAppService = corporateAppService;
    }

    [HttpGet("get-all-apps")]
    public Task<IActionResult> GetAll(CancellationToken cancellationToken) 
        => ExecuteAsync(_corporateAppService.GetAllAsync, cancellationToken);
}
