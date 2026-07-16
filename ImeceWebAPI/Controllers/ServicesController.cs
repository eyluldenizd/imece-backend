using Application.DTOs;
using Application.Services;
using Core.Common;
using ImeceWebAPI.Controllers.Common;
using Microsoft.AspNetCore.Mvc;

namespace ImeceWebAPI.Controllers;

[ApiController]
[Route("api/services/")]
public sealed class ServicesController : ApiControllerBase
{
    private readonly ServicesService _servicesService;

    public ServicesController(ServicesService servicesService)
    {
        _servicesService = servicesService;
    }

    [HttpGet("get-all-services")]
    public Task<IActionResult> GetAll(CancellationToken cancellationToken) 
        => ExecuteAsync(_servicesService.GetAllAsync, cancellationToken);

    [HttpGet("get-active-services")]
    public Task<IActionResult> GetActive(CancellationToken cancellationToken) 
        => ExecuteAsync(_servicesService.GetActiveAsync, cancellationToken);

    [HttpGet("get-service-by-id/{id:long}")]
    public Task<IActionResult> GetById(long id, CancellationToken cancellationToken) 
        => ExecuteAsync(new IdRequest { Id = id }, _servicesService.GetByIdAsync, cancellationToken);

    [HttpPost("create-service")]
    public Task<IActionResult> Create([FromBody] CreateServiceDto request, CancellationToken cancellationToken) 
        => ExecuteAsync(request, _servicesService.CreateAsync, cancellationToken);

    [HttpPut("update-service-by-id/{id:long}")]
    public Task<IActionResult> Update(long id, [FromBody] UpdateServiceDto request, CancellationToken cancellationToken)
    {
        request.ServiceId = id;
        return ExecuteAsync(request, _servicesService.UpdateAsync, cancellationToken);
    }

    [HttpDelete("get-service-passive/{id:long}")]
    public Task<IActionResult> Delete(long id, CancellationToken cancellationToken) 
        => ExecuteAsync(new IdRequest { Id = id }, _servicesService.DeleteAsync, cancellationToken);
}
