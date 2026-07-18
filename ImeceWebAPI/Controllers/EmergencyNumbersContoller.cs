using Application.DTOs;
using Application.Services;
using Core.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = ImecePolicies.RequireRegisteredUser)]
public class EmergencyNumbersController : ControllerBase
{
    private readonly EmergencyNumberService _service;

    public EmergencyNumbersController(
        EmergencyNumberService service)
    {
        _service = service;
    }


    // GET: api/emergencynumbers
    [HttpGet]
    public async Task<IActionResult> GetAll(
        CancellationToken cancellationToken)
    {
        var result = await _service.GetAllAsync(
            cancellationToken);

        return Ok(result);
    }


    // GET: api/emergencynumbers/{id}
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


    // POST: api/emergencynumbers
    [HttpPost]
    [Authorize(Policy = ImecePolicies.RequireCompanyAdmin)]
    public async Task<IActionResult> Create(
        [FromBody] EmergencyNumberDto dto,
        CancellationToken cancellationToken)
    {
        await _service.CreateAsync(
            dto,
            cancellationToken);

        return Ok(new
        {
            message = "Emergency number created successfully"
        });
    }


    // PUT: api/emergencynumbers/{id}
    [HttpPut("{id:long}")]
    [Authorize(Policy = ImecePolicies.RequireCompanyAdmin)]
    public async Task<IActionResult> Update(
        long id,
        [FromBody] EmergencyNumberDto dto,
        CancellationToken cancellationToken)
    {
        dto.EmergencyNumberId = id;

        await _service.UpdateAsync(
            dto,
            cancellationToken);

        return Ok(new
        {
            message = "Emergency number updated successfully"
        });
    }


    // DELETE: api/emergencynumbers/{id}
    [HttpDelete("{id:long}")]
    [Authorize(Policy = ImecePolicies.RequireCompanyAdmin)]
    public async Task<IActionResult> Delete(
        long id,
        CancellationToken cancellationToken)
    {
        await _service.DeleteAsync(
            id,
            cancellationToken);

        return Ok(new
        {
            message = "Emergency number deleted successfully"
        });
    }
}