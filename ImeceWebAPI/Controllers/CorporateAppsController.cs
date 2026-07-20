using Application.DTOs;
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

    [HttpGet("get-app-by-id/{id:long}")]
    public Task<IActionResult> GetById(long id, CancellationToken cancellationToken)
        => ExecuteAsync(new IdRequest { Id = id }, _corporateAppService.GetByIdAsync, cancellationToken);

    [HttpPost("create-app")]
    [Authorize(Policy = ImecePolicies.RequireCompanyAdminOrGlobalContentManager)]
    public Task<IActionResult> Create([FromBody] CreateCorporateAppDto request, CancellationToken cancellationToken)
        => ExecuteAsync(request, _corporateAppService.CreateAsync, cancellationToken);

    [HttpPut("update-app-by-id/{id:long}")]
    [Authorize(Policy = ImecePolicies.RequireCompanyAdminOrGlobalContentManager)]
    public Task<IActionResult> Update(long id, [FromBody] UpdateCorporateAppDto request, CancellationToken cancellationToken)
    {
        request.AppId = id;
        return ExecuteAsync(request, _corporateAppService.UpdateAsync, cancellationToken);
    }

    [HttpDelete("delete-app-by-id/{id:long}")]
    [Authorize(Policy = ImecePolicies.RequireCompanyAdminOrGlobalContentManager)]
    public Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
        => ExecuteAsync(new IdRequest { Id = id }, _corporateAppService.DeleteAsync, cancellationToken);
}
