using Application.DTOs;
using Application.Services;
using Core.Authorization;
using Core.Common;
using ImeceWebAPI.Controllers.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ImeceWebAPI.Controllers;

[ApiController]
[Route("api/social-activities/")]
[Authorize(Policy = ImecePolicies.RequireRegisteredUser)]
public sealed class SocialActivitiesController : ApiControllerBase
{
    private readonly SocialActivityService _socialActivityService;

    public SocialActivitiesController(SocialActivityService socialActivityService)
    {
        _socialActivityService = socialActivityService;
    }

    [HttpGet("get-all")]
    public Task<IActionResult> GetAll(CancellationToken cancellationToken)
        => ExecuteAsync(_socialActivityService.GetAllAsync, cancellationToken);

    [HttpGet("get-by-id/{id:long}")]
    public Task<IActionResult> GetById(long id, CancellationToken cancellationToken)
        => ExecuteAsync(new IdRequest { Id = id }, _socialActivityService.GetByIdAsync, cancellationToken);

    [HttpPost("create")]
    [Authorize(Policy = ImecePolicies.RequireCompanyAdmin)]
    public Task<IActionResult> Create([FromBody] CreateSocialActivityDto request, CancellationToken cancellationToken)
        => ExecuteAsync(request, _socialActivityService.CreateAsync, cancellationToken);

    [HttpPut("update-by-id/{id:long}")]
    [Authorize(Policy = ImecePolicies.RequireCompanyAdmin)]
    public Task<IActionResult> Update(long id, [FromBody] UpdateSocialActivityDto request, CancellationToken cancellationToken)
    {
        request.SocialActivityId = id;
        return ExecuteAsync(request, _socialActivityService.UpdateAsync, cancellationToken);
    }

    [HttpDelete("delete-by-id/{id:long}")]
    [Authorize(Policy = ImecePolicies.RequireCompanyAdmin)]
    public Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
        => ExecuteAsync(new IdRequest { Id = id }, _socialActivityService.DeleteAsync, cancellationToken);
}
