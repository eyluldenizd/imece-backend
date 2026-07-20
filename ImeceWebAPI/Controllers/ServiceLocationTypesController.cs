using Application.DTOs;
using Application.Services;
using Core.Authorization;
using ImeceWebAPI.Controllers.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ImeceWebAPI.Controllers;

[ApiController]
[Route("api/service-location-types/")]
[Authorize(Policy = ImecePolicies.RequireRegisteredUser)]
public sealed class ServiceLocationTypesController : ApiControllerBase
{
    private readonly ServiceLocationTypeService _serviceLocationTypeService;

    public ServiceLocationTypesController(ServiceLocationTypeService serviceLocationTypeService)
    {
        _serviceLocationTypeService = serviceLocationTypeService;
    }

    [HttpGet("")]
    public Task<IActionResult> GetAll(CancellationToken cancellationToken)
        => ExecuteAsync(_serviceLocationTypeService.GetAllAsync, cancellationToken);

    [HttpGet("{id:int}")]
    public Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        => ExecuteAsync(new IdRequest { Id = id }, _serviceLocationTypeService.GetByIdAsync, cancellationToken);

    [HttpPost("")]
    [Authorize(Policy = ImecePolicies.RequireGlobalContentManager)]
    public Task<IActionResult> Create(
        [FromBody] CreateServiceLocationTypeDto request,
        CancellationToken cancellationToken)
        => ExecuteAsync(request, _serviceLocationTypeService.CreateAsync, cancellationToken);

    [HttpPut("{id:int}")]
    [Authorize(Policy = ImecePolicies.RequireGlobalContentManager)]
    public Task<IActionResult> Update(
        int id,
        [FromBody] UpdateServiceLocationTypeDto request,
        CancellationToken cancellationToken)
    {
        request.ServiceLocationTypeId = id;
        return ExecuteAsync(request, _serviceLocationTypeService.UpdateAsync, cancellationToken);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = ImecePolicies.RequireGlobalContentManager)]
    public Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        => ExecuteAsync(new IdRequest { Id = id }, _serviceLocationTypeService.DeleteAsync, cancellationToken);
}
