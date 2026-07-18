using Application.DTOs;
using Application.Services;
using Core.Authorization;
using ImeceWebAPI.Controllers.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ImeceWebAPI.Controllers;

[ApiController]
[Route("api/reservations/")]
[Authorize(Policy = ImecePolicies.RequireRegisteredUser)]
public sealed class ReservationsController : ApiControllerBase
{
    private readonly ReservationService _reservationService;

    public ReservationsController(
        ReservationService reservationService)
    {
        _reservationService = reservationService;
    }

    [HttpGet("get-all-reservations")]
    public Task<IActionResult> GetAll(
        CancellationToken cancellationToken)
    {
        return ExecuteAsync(
            _reservationService.GetAllAsync,
            cancellationToken);
    }

    [HttpGet("get-reservation-by-id/{id:long}")]
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
            _reservationService.GetByIdAsync,
            cancellationToken);
    }

    [HttpGet("get-reservations-by-organizer/{organizerUserId:long}")]
    public Task<IActionResult> GetByOrganizer(
        long organizerUserId,
        CancellationToken cancellationToken)
    {
        return ExecuteAsync(
            cancellationToken => _reservationService.GetByOrganizerAsync(organizerUserId, cancellationToken),
            cancellationToken);
    }

    [HttpGet("get-reservations-by-room/{roomName}")]
    public Task<IActionResult> GetByRoomName(
        string roomName,
        CancellationToken cancellationToken)
    {
        return ExecuteAsync(
            cancellationToken => _reservationService.GetByRoomNameAsync(roomName, cancellationToken),
            cancellationToken);
    }

    [HttpPost("create-reservation")]
    [Authorize(Policy = ImecePolicies.RequireCompanyAdmin)]
    public Task<IActionResult> Create(
        [FromBody] CreateReservationDto request,
        CancellationToken cancellationToken)
    {
        return ExecuteAsync(
            request,
            _reservationService.CreateAsync,
            cancellationToken);
    }

    [HttpPut("update-reservation-by-id/{id:long}")]
    [Authorize(Policy = ImecePolicies.RequireCompanyAdmin)]
    public Task<IActionResult> Update(
        long id,
        [FromBody] UpdateReservationDto request,
        CancellationToken cancellationToken)
    {
        request.ReservationId = id;

        return ExecuteAsync(
            request,
            _reservationService.UpdateAsync,
            cancellationToken);
    }

    [HttpPatch("update-reservation-status/{id:long}")]
    [Authorize(Policy = ImecePolicies.RequireCompanyAdmin)]
    public Task<IActionResult> UpdateStatus(
        long id,
        [FromBody] string status,
        CancellationToken cancellationToken)
    {
        return ExecuteAsync(
            cancellationToken => _reservationService.UpdateStatusAsync(id, status, cancellationToken),
            cancellationToken);
    }

    [HttpDelete("delete-reservation/{id:long}")]
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
            _reservationService.DeleteAsync,
            cancellationToken);
    }
}