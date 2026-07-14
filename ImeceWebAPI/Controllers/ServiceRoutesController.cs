using Application.DTOs;
using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServiceRoutesController : ControllerBase
{
    private readonly ServiceRouteService _service;

    public ServiceRoutesController(
        ServiceRouteService service)
    {
        _service = service;
    }


    // GET: api/serviceroutes
    [HttpGet]
    public async Task<IActionResult> GetAll(
        CancellationToken cancellationToken)
    {
        var result = await _service.GetAllAsync(
            cancellationToken);

        return Ok(result);
    }


    // GET: api/serviceroutes/{id}
    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(
        long id,
        CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(
            id,
            cancellationToken);

        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }


    // POST: api/serviceroutes
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] ServiceRouteDto dto,
        CancellationToken cancellationToken)
    {
        await _service.CreateAsync(
            dto,
            cancellationToken);

        return Ok(new
        {
            message = "Service route created successfully"
        });
    }


    // PUT: api/serviceroutes/{id}
    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(
        long id,
        [FromBody] ServiceRouteDto dto,
        CancellationToken cancellationToken)
    {
        dto.ServiceRouteId = id;

        await _service.UpdateAsync(
            dto,
            cancellationToken);

        return Ok(new
        {
            message = "Service route updated successfully"
        });
    }


    // DELETE: api/serviceroutes/{id}
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(
        long id,
        CancellationToken cancellationToken)
    {
        await _service.DeleteAsync(
            id,
            cancellationToken);

        return Ok(new
        {
            message = "Service route deleted successfully"
        });
    }
}