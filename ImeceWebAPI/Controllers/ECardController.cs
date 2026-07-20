using Application.DTOs;
using Application.Services;
using Core.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ImeceWebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = ImecePolicies.RequireRegisteredUser)]
public class ECardsController : ControllerBase
{
    private readonly ECardService _service;

    public ECardsController(
        ECardService service)
    {
        _service = service;
    }


    // GET: api/ECards
    [HttpGet]
    public async Task<IActionResult> GetAll(
        CancellationToken cancellationToken)
    {
        var result = await _service.GetAllAsync(
            cancellationToken);

        return Ok(result);
    }


    // GET: api/ECards/1
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


    // POST: api/ECards
    [HttpPost]
    [Authorize(Policy = ImecePolicies.RequireCompanyAdmin)]
    public async Task<IActionResult> Create(
        [FromBody] ECardDto dto,
        CancellationToken cancellationToken)
    {
        await _service.CreateAsync(
            dto,
            cancellationToken);

        return Ok(new
        {
            message = "E-Card created successfully"
        });
    }


    // PUT: api/ECards/1
    [HttpPut("{id:long}")]
    [Authorize(Policy = ImecePolicies.RequireCompanyAdmin)]
    public async Task<IActionResult> Update(
        long id,
        [FromBody] ECardDto dto,
        CancellationToken cancellationToken)
    {
        dto.ECardId = id;

        await _service.UpdateAsync(
            dto,
            cancellationToken);

        return Ok(new
        {
            message = "E-Card updated successfully"
        });
    }


    // DELETE: api/ECards/1
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
            message = "E-Card deleted successfully"
        });
    }
}