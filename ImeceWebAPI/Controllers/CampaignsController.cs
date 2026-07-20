using Application.DTOs;
using Application.Services;
using Core.Authorization;
using Core.Common;
using ImeceWebAPI.Controllers.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ImeceWebAPI.Controllers;

[ApiController]
[Route("api/campaigns/")]
[Authorize(Policy = ImecePolicies.RequireRegisteredUser)]
public sealed class CampaignsController : ApiControllerBase
{
    private readonly CampaignService _campaignService;

    public CampaignsController(CampaignService campaignService)
    {
        _campaignService = campaignService;
    }

    [HttpGet("get-all-campaigns")]
    public Task<IActionResult> GetAll(CancellationToken cancellationToken) 
        => ExecuteAsync(_campaignService.GetAllAsync, cancellationToken);

    [HttpGet("get-active-campaigns")]
    public Task<IActionResult> GetActive(CancellationToken cancellationToken) 
        => ExecuteAsync(_campaignService.GetActiveAsync, cancellationToken);

    [HttpGet("get-campaign-by-id/{id:long}")]
    public Task<IActionResult> GetById(long id, CancellationToken cancellationToken) 
        => ExecuteAsync(new IdRequest { Id = id }, _campaignService.GetByIdAsync, cancellationToken);

    [HttpPost("create-campaign")]
    [Authorize(Policy = ImecePolicies.RequireCompanyAdmin)]
    public Task<IActionResult> Create([FromBody] CreateCampaignDto request, CancellationToken cancellationToken) 
        => ExecuteAsync(request, _campaignService.CreateAsync, cancellationToken);

    [HttpPut("update-campaign-by-id/{id:long}")]
    [Authorize(Policy = ImecePolicies.RequireCompanyAdmin)]
    public Task<IActionResult> Update(long id, [FromBody] UpdateCampaignDto request, CancellationToken cancellationToken)
    {
        request.CampaignId = id;
        return ExecuteAsync(request, _campaignService.UpdateAsync, cancellationToken);
    }

    [HttpDelete("get-campaign-passive/{id:long}")]
    [Authorize(Policy = ImecePolicies.RequireCompanyAdmin)]
    public Task<IActionResult> Delete(long id, CancellationToken cancellationToken) 
        => ExecuteAsync(new IdRequest { Id = id }, _campaignService.DeleteAsync, cancellationToken);
}
