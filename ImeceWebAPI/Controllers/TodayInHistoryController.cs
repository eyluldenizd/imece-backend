using Application.DTOs;
using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TodayInHistoryController : ControllerBase
{
    private readonly TodayInHistoryService _service;

    public TodayInHistoryController(
        TodayInHistoryService service)
    {
        _service = service;
    }


    // GET: api/todayinhistory
    [HttpGet]
    public async Task<IActionResult> GetAll(
        CancellationToken cancellationToken)
    {
        var result = await _service.GetAllAsync(
            cancellationToken);

        return Ok(result);
    }


    // POST: api/todayinhistory
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] TodayInHistoryDto dto,
        CancellationToken cancellationToken)
    {
        await _service.CreateAsync(
            dto,
            cancellationToken);

        return Ok(new
        {
            message = "Today in history created successfully"
        });
    }


    // PUT: api/todayinhistory/{id}
    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(
        long id,
        [FromBody] TodayInHistoryDto dto,
        CancellationToken cancellationToken)
    {
        dto.Id = id;

        await _service.UpdateAsync(
            dto,
            cancellationToken);

        return Ok(new
        {
            message = "Today in history updated successfully"
        });
    }


    // DELETE: api/todayinhistory/{id}
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
            message = "Today in history deleted successfully"
        });
    }
}