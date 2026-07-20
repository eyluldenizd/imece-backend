using Application.Services;
using Core.Authorization;
using ImeceWebAPI.Controllers.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ImeceWebAPI.Controllers;

[ApiController]
[Route("api/upcoming-events/")]
[Authorize(Policy = ImecePolicies.RequireRegisteredUser)]
public sealed class UpcomingEventsController : ApiControllerBase
{
    private readonly UpcomingEventService _upcomingEventService;

    public UpcomingEventsController(UpcomingEventService upcomingEventService)
    {
        _upcomingEventService = upcomingEventService;
    }

    [HttpGet("get-all-events")]
    public Task<IActionResult> GetAll(CancellationToken cancellationToken) 
        => ExecuteAsync(_upcomingEventService.GetAllAsync, cancellationToken);
}
