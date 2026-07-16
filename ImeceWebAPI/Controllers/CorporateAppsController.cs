using Application.Services;
using ImeceWebAPI.Controllers.Common;
using Microsoft.AspNetCore.Mvc;

namespace ImeceWebAPI.Controllers;

[ApiController]
[Route("api/corporate-apps/")]
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
