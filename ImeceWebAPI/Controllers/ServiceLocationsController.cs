using Application.DTOs;
using Application.Services;
using Core.Authorization;
using Core.Common;
using ImeceWebAPI.Controllers.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ImeceWebAPI.Controllers;

[ApiController]
[Route("api/service-locations/")]
[Authorize(Policy = ImecePolicies.RequireRegisteredUser)]
public sealed class ServiceLocationsController : ApiControllerBase
{
    private readonly ServiceLocationService _serviceLocationService;

    public ServiceLocationsController(ServiceLocationService serviceLocationService)
    {
        _serviceLocationService = serviceLocationService;
    }

    [HttpGet("get-all")]
    public Task<IActionResult> GetAll(CancellationToken cancellationToken)
        => ExecuteAsync(_serviceLocationService.GetAllAsync, cancellationToken);

    [HttpGet("get-by-id/{id:long}")]
    public Task<IActionResult> GetById(long id, CancellationToken cancellationToken)
        => ExecuteAsync(new IdRequest { Id = id }, _serviceLocationService.GetByIdAsync, cancellationToken);

    [HttpPost("create")]
    [Authorize(Policy = ImecePolicies.RequireCompanyAdmin)]
    public Task<IActionResult> Create([FromBody] CreateServiceLocationDto request, CancellationToken cancellationToken)
        => ExecuteAsync(request, _serviceLocationService.CreateAsync, cancellationToken);

    [HttpPut("update-by-id/{id:long}")]
    [Authorize(Policy = ImecePolicies.RequireCompanyAdmin)]
    public Task<IActionResult> Update(long id, [FromBody] UpdateServiceLocationDto request, CancellationToken cancellationToken)
    {
        request.ServiceLocationId = id;
        return ExecuteAsync(request, _serviceLocationService.UpdateAsync, cancellationToken);
    }

    [HttpDelete("delete-by-id/{id:long}")]
    [Authorize(Policy = ImecePolicies.RequireCompanyAdmin)]
    public Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
        => ExecuteAsync(new IdRequest { Id = id }, _serviceLocationService.DeleteAsync, cancellationToken);
}
