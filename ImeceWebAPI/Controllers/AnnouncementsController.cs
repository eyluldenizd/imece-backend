using Application.DTOs;
using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace ImeceWebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AnnouncementsController : ControllerBase
{
    private readonly AnnouncementService _announcementService;

    public AnnouncementsController(
        AnnouncementService announcementService)
    {
        _announcementService = announcementService;
    }

    [HttpGet("get-all-announcements")]
    [ProducesResponseType(
        typeof(List<AnnouncementDto>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AnnouncementDto>>>
        GetAll(
            CancellationToken cancellationToken)
    {
        var announcements =
            await _announcementService.GetAllAsync(
                cancellationToken);

        return Ok(announcements);
    }
    
    [HttpGet("get-published-annoucements")]
    [ProducesResponseType(
        typeof(List<AnnouncementDto>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AnnouncementDto>>>
        GetPublished(
            CancellationToken cancellationToken)
    {
        var announcements =
            await _announcementService.GetPublishedAsync(
                cancellationToken);

        return Ok(announcements);
    }

    [HttpGet("{id:long}")]
    [ProducesResponseType(
        typeof(AnnouncementDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AnnouncementDto>>
        GetById(
            long id,
            CancellationToken cancellationToken)
    {
        var announcement =
            await _announcementService.GetByIdAsync(
                id,
                cancellationToken);

        return Ok(announcement);
    }

    [HttpPost]
    [ProducesResponseType(
        StatusCodes.Status201Created)]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateAnnouncementDto request,
        CancellationToken cancellationToken)
    {
        await _announcementService.CreateAsync(
            request,
            cancellationToken);

        return StatusCode(
            StatusCodes.Status201Created);
    }

    [HttpPut("{id:long}")]
    [ProducesResponseType(
        StatusCodes.Status204NoContent)]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        long id,
        [FromBody] UpdateAnnouncementDto request,
        CancellationToken cancellationToken)
    {
        // Ensure the DTO's ID matches the route ID
        request.AnnouncementId = id;

        await _announcementService.UpdateAsync(
            request,
            cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id:long}")]
    [ProducesResponseType(
        StatusCodes.Status204NoContent)]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        long id,
        CancellationToken cancellationToken)
    {
        await _announcementService.DeleteAsync(
            id,
            cancellationToken);

        return NoContent();
    }
}