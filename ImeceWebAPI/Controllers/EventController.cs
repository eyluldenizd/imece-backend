using Application.DTOs;
using Application.Services;
using Core.Authorization;
using ImeceWebAPI.Controllers.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ImeceWebAPI.Controllers;

[ApiController]
[Route("api/events/")]
[Authorize(Policy = ImecePolicies.RequireRegisteredUser)]
public sealed class EventsController : ApiControllerBase
{
    private readonly EventService _eventService;

    public EventsController(
        EventService eventService)
    {
        _eventService = eventService;
    }

    [HttpGet("get-all-events")]
    public Task<IActionResult> GetAll(
        CancellationToken cancellationToken)
    {
        return ExecuteAsync(
            _eventService.GetAllAsync,
            cancellationToken);
    }

    [HttpGet("get-upcoming-events")]
    public Task<IActionResult> GetUpcoming(
        CancellationToken cancellationToken)
    {
        return ExecuteAsync(
            _eventService.GetUpcomingAsync,
            cancellationToken);
    }

    [HttpGet("get-event-by-id/{id:long}")]
    public Task<IActionResult> GetById(
        long id,
        CancellationToken cancellationToken)
    {
        var request = new IdRequest
        {
            Id = id
        };

        return ExecuteAsync(
            request,
            _eventService.GetByIdAsync,
            cancellationToken);
    }

    [HttpPost("create-event")]
    [Authorize(Policy = ImecePolicies.RequireCompanyAdmin)]
    public Task<IActionResult> Create(
        [FromBody] CreateEventDto request,
        CancellationToken cancellationToken)
    {
        return ExecuteAsync(
            request,
            _eventService.CreateAsync,
            cancellationToken);
    }

    [HttpPut("update-event-by-id/{id:long}")]
    [Authorize(Policy = ImecePolicies.RequireCompanyAdmin)]
    public Task<IActionResult> Update(
        long id,
        [FromBody] UpdateEventDto request,
        CancellationToken cancellationToken)
    {
        request.EventId = id;

        return ExecuteAsync(
            request,
            _eventService.UpdateAsync,
            cancellationToken);
    }

    [HttpDelete("get-event-passive/{id:long}")]
    [Authorize(Policy = ImecePolicies.RequireCompanyAdmin)]
    public Task<IActionResult> Delete(
        long id,
        CancellationToken cancellationToken)
    {
        var request = new IdRequest
        {
            Id = id
        };

        return ExecuteAsync(
            request,
            _eventService.DeleteAsync,
            cancellationToken);
    }
}