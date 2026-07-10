using Application.DTOs;
using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace ImeceWebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnnouncementsController : ControllerBase
{
    private readonly AnnouncementService _announcementService;

    public AnnouncementsController(AnnouncementService announcementService)
    {
        _announcementService = announcementService;
    }

    [HttpGet]
    public async Task<ActionResult<List<AnnouncementDto>>> GetAll()
    {
        var announcements = await _announcementService.GetAllAsync();

        return Ok(announcements);
    }
}