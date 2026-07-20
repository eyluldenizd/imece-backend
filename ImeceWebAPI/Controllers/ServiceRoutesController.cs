using Application.DTOs;
using Application.Services;
using Core.Authorization;
using Core.Common;
using ImeceWebAPI.Controllers.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ImeceWebAPI.Controllers;

[ApiController]
[Route("api/service-routes/")]
[Authorize(Policy = ImecePolicies.RequireRegisteredUser)]
public sealed class ServiceRoutesController : ApiControllerBase
{
    private readonly ServiceRouteService _serviceRouteService;

    public ServiceRoutesController(ServiceRouteService serviceRouteService)
    {
        _serviceRouteService = serviceRouteService;
    }

    [HttpGet("get-all")]
    public Task<IActionResult> GetAll(CancellationToken cancellationToken)
        => ExecuteAsync(_serviceRouteService.GetAllAsync, cancellationToken);

    [HttpGet("get-by-id/{id:long}")]
    public Task<IActionResult> GetById(long id, CancellationToken cancellationToken)
        => ExecuteAsync(new IdRequest { Id = id }, _serviceRouteService.GetByIdAsync, cancellationToken);

    [HttpPost("create")]
    [Authorize(Policy = ImecePolicies.RequireCompanyAdmin)]
    public Task<IActionResult> Create([FromBody] CreateServiceRouteDto request, CancellationToken cancellationToken)
        => ExecuteAsync(request, _serviceRouteService.CreateAsync, cancellationToken);

    [HttpPut("update-by-id/{id:long}")]
    [Authorize(Policy = ImecePolicies.RequireCompanyAdmin)]
    public Task<IActionResult> Update(long id, [FromBody] UpdateServiceRouteDto request, CancellationToken cancellationToken)
    {
        request.ServiceRouteId = id;
        return ExecuteAsync(request, _serviceRouteService.UpdateAsync, cancellationToken);
    }

    [HttpDelete("delete-by-id/{id:long}")]
    [Authorize(Policy = ImecePolicies.RequireCompanyAdmin)]
    public Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
        => ExecuteAsync(new IdRequest { Id = id }, _serviceRouteService.DeleteAsync, cancellationToken);
}
