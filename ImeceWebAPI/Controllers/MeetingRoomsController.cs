using Application.DTOs;
using Application.Services;
using Core.Authorization;
using Core.Common;
using ImeceWebAPI.Controllers.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ImeceWebAPI.Controllers;

[ApiController]
[Route("api/meeting-rooms/")]
[Authorize(Policy = ImecePolicies.RequireRegisteredUser)]
public sealed class MeetingRoomsController : ApiControllerBase
{
    private readonly MeetingRoomService _meetingRoomService;

    public MeetingRoomsController(MeetingRoomService meetingRoomService)
    {
        _meetingRoomService = meetingRoomService;
    }

    [HttpGet("get-all")]
    [Authorize(Policy = ImecePolicies.RequireReservationsView)]
    public Task<IActionResult> GetAll(
        [FromQuery] ContentListQueryDto query,
        CancellationToken cancellationToken)
        => ExecuteAsync(
            token => _meetingRoomService.GetAllAsync(query, token),
            cancellationToken);

    [HttpGet("get-by-id/{id:int}")]
    [Authorize(Policy = ImecePolicies.RequireReservationsView)]
    public Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        => ExecuteAsync(new IdRequest { Id = id }, _meetingRoomService.GetByIdAsync, cancellationToken);

    [HttpPost("create")]
    [Authorize(Policy = ImecePolicies.RequireReservationsManage)]
    public Task<IActionResult> Create([FromBody] CreateMeetingRoomDto request, CancellationToken cancellationToken)
        => ExecuteAsync(request, _meetingRoomService.CreateAsync, cancellationToken);

    [HttpPut("update-by-id/{id:int}")]
    [Authorize(Policy = ImecePolicies.RequireReservationsManage)]
    public Task<IActionResult> Update(int id, [FromBody] UpdateMeetingRoomDto request, CancellationToken cancellationToken)
    {
        request.MeetingRoomId = id;
        return ExecuteAsync(request, _meetingRoomService.UpdateAsync, cancellationToken);
    }

    [HttpDelete("delete-by-id/{id:int}")]
    [Authorize(Policy = ImecePolicies.RequireReservationsManage)]
    public Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        => ExecuteAsync(new IdRequest { Id = id }, _meetingRoomService.DeleteAsync, cancellationToken);
}
