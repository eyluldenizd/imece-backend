using Application.DTOs;

using Core.Common;
using Infrastructure.Entities;
using Infrastructure.Repositories;

namespace Application.Services;

public sealed class ReservationService
{
    private readonly ReservationRepository _reservationRepository;

    public ReservationService(
        ReservationRepository reservationRepository)
    {
        _reservationRepository = reservationRepository;
    }

    public async Task<ServiceResult<IReadOnlyList<ReservationDto>>>
        GetAllAsync(
            CancellationToken cancellationToken = default)
    {
        var reservations = await _reservationRepository.GetAllAsync(
            cancellationToken);

        IReadOnlyList<ReservationDto> response = reservations
            .Select(ToDto)
            .ToList();

        return ServiceResult<IReadOnlyList<ReservationDto>>
            .Success(response);
    }

    public async Task<ServiceResult<ReservationDto>> GetByIdAsync(
        IdRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _reservationRepository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (entity is null)
        {
            return ServiceResult<ReservationDto>.NotFound(
                $"ID değeri {request.Id} olan rezervasyon bulunamadı.");
        }

        return ServiceResult<ReservationDto>.Success(
            ToDto(entity));
    }

    public async Task<ServiceResult<IReadOnlyList<ReservationDto>>>
        GetByOrganizerAsync(
            long organizerUserId,
            CancellationToken cancellationToken = default)
    {
        var reservations = await _reservationRepository.GetByOrganizerAsync(
            organizerUserId,
            cancellationToken);

        IReadOnlyList<ReservationDto> response = reservations
            .Select(ToDto)
            .ToList();

        return ServiceResult<IReadOnlyList<ReservationDto>>
            .Success(response);
    }

    public async Task<ServiceResult<IReadOnlyList<ReservationDto>>>
        GetByRoomNameAsync(
            string roomName,
            CancellationToken cancellationToken = default)
    {
        var reservations = await _reservationRepository.GetByRoomNameAsync(
            roomName,
            cancellationToken);

        IReadOnlyList<ReservationDto> response = reservations
            .Select(ToDto)
            .ToList();

        return ServiceResult<IReadOnlyList<ReservationDto>>
            .Success(response);
    }

    public async Task<ServiceResult<long>> CreateAsync(
        CreateReservationDto request,
        CancellationToken cancellationToken = default)
    {
        if (request.StartTime >= request.EndTime)
        {
            return ServiceResult<long>.Conflict(
                "Başlangıç zamanı, bitiş zamanından önce olmalıdır.");
        }

        var overlaps = await _reservationRepository.CheckOverlapAsync(
            request.RoomName,
            request.StartTime,
            request.EndTime,
            excludeReservationId: 0,
            cancellationToken);

        if (overlaps.Count > 0)
        {
            return ServiceResult<long>.Conflict(
                $"'{request.RoomName}' odası seçilen saat aralığında zaten dolu.");
        }

        var entity = new Reservation
        {
            RoomName = request.RoomName,
            OrganizerUserId = request.OrganizerUserId,
            Title = request.Title,
            Description = request.Description,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            Status = "pending"
        };

        var reservationId = await _reservationRepository.CreateAsync(
            entity,
            cancellationToken);

        return ServiceResult<long>.Created(reservationId);
    }

    public async Task<ServiceResult> UpdateAsync(
        UpdateReservationDto request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _reservationRepository.GetByIdAsync(
            request.ReservationId,
            cancellationToken);

        if (entity is null)
        {
            return ServiceResult.NotFound(
                $"ID değeri {request.ReservationId} olan rezervasyon bulunamadı.");
        }

        if (request.StartTime >= request.EndTime)
        {
            return ServiceResult.Conflict(
                "Başlangıç zamanı, bitiş zamanından önce olmalıdır.");
        }

        var overlaps = await _reservationRepository.CheckOverlapAsync(
            request.RoomName,
            request.StartTime,
            request.EndTime,
            excludeReservationId: request.ReservationId,
            cancellationToken);

        if (overlaps.Count > 0)
        {
            return ServiceResult.Conflict(
                $"'{request.RoomName}' odası seçilen saat aralığında zaten dolu.");
        }

        entity.RoomName = request.RoomName;
        entity.OrganizerUserId = request.OrganizerUserId;
        entity.Title = request.Title;
        entity.Description = request.Description;
        entity.StartTime = request.StartTime;
        entity.EndTime = request.EndTime;
        entity.Status = request.Status;

        var rowsAffected = await _reservationRepository.UpdateAsync(
            entity,
            cancellationToken);

        if (rowsAffected == 0)
        {
            return ServiceResult.Conflict(
                "Rezervasyon güncellenemedi.");
        }

        return ServiceResult.NoContent();
    }

    public async Task<ServiceResult> UpdateStatusAsync(
        long id,
        string status,
        CancellationToken cancellationToken = default)
    {
        var entity = await _reservationRepository.GetByIdAsync(
            id,
            cancellationToken);

        if (entity is null)
        {
            return ServiceResult.NotFound(
                $"ID değeri {id} olan rezervasyon bulunamadı.");
        }

        var rowsAffected = await _reservationRepository.UpdateStatusAsync(
            id,
            status,
            cancellationToken);

        if (rowsAffected == 0)
        {
            return ServiceResult.Conflict(
                "Rezervasyon durumu güncellenemedi.");
        }

        return ServiceResult.NoContent();
    }

    public async Task<ServiceResult> DeleteAsync(
        IdRequest request,
        CancellationToken cancellationToken = default)
    {
        var rowsAffected = await _reservationRepository.DeleteAsync(
            request.Id,
            cancellationToken);

        if (rowsAffected == 0)
        {
            return ServiceResult.NotFound(
                $"ID değeri {request.Id} olan rezervasyon bulunamadı.");
        }

        return ServiceResult.NoContent();
    }

    private static ReservationDto ToDto(
        Reservation entity)
    {
        return new ReservationDto
        {
            ReservationId = entity.ReservationId,
            RoomName = entity.RoomName,
            OrganizerUserId = entity.OrganizerUserId,
            Title = entity.Title,
            Description = entity.Description,
            StartTime = entity.StartTime,
            EndTime = entity.EndTime,
            Status = entity.Status,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
}