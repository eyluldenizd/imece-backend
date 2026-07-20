using Application.DTOs;
using Core.Authorization;
using Core.Common;
using Infrastructure.Entities;
using Infrastructure.Repositories;

namespace Application.Services;

public sealed class ReservationService
{
    private readonly ReservationRepository _reservationRepository;
    private readonly MeetingRoomRepository _meetingRoomRepository;
    private readonly ICompanyContext _companyContext;

    public ReservationService(
        ReservationRepository reservationRepository,
        MeetingRoomRepository meetingRoomRepository,
        ICompanyContext companyContext)
    {
        _reservationRepository = reservationRepository;
        _meetingRoomRepository = meetingRoomRepository;
        _companyContext = companyContext;
    }

    public async Task<ServiceResult<IReadOnlyList<ReservationDto>>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var reservations = await _reservationRepository.GetAllAsync(cancellationToken);
        return ServiceResult<IReadOnlyList<ReservationDto>>.Success(
            reservations.Select(ToDto).ToList());
    }

    public async Task<ServiceResult<ReservationDto>> GetByIdAsync(
        IdRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _reservationRepository.GetByIdAsync(request.Id, cancellationToken);
        if (entity is null)
        {
            return ServiceResult<ReservationDto>.NotFound(
                $"ID değeri {request.Id} olan rezervasyon bulunamadı.");
        }

        return ServiceResult<ReservationDto>.Success(ToDto(entity));
    }

    public async Task<ServiceResult<IReadOnlyList<ReservationDto>>> GetByOrganizerAsync(
        long organizerUserId,
        CancellationToken cancellationToken = default)
    {
        var reservations = await _reservationRepository.GetByOrganizerAsync(
            organizerUserId,
            cancellationToken);

        return ServiceResult<IReadOnlyList<ReservationDto>>.Success(
            reservations.Select(ToDto).ToList());
    }

    public async Task<ServiceResult<IReadOnlyList<ReservationDto>>> GetByRoomNameAsync(
        string roomName,
        CancellationToken cancellationToken = default)
    {
        var reservations = await _reservationRepository.GetByRoomNameAsync(
            roomName,
            cancellationToken);

        return ServiceResult<IReadOnlyList<ReservationDto>>.Success(
            reservations.Select(ToDto).ToList());
    }

    public async Task<ServiceResult<long>> CreateAsync(
        CreateReservationDto request,
        CancellationToken cancellationToken = default)
    {
        var buildResult = await BuildReservationAsync(request, cancellationToken);
        if (buildResult.Error is not null)
        {
            return buildResult.Error;
        }

        var entity = buildResult.Entity!;

        if (request.StartTime >= request.EndTime)
        {
            return ServiceResult<long>.Conflict(
                "Başlangıç zamanı, bitiş zamanından önce olmalıdır.");
        }

        var overlap = await HasOverlapAsync(
            entity.MeetingRoomId,
            entity.RoomName,
            request.StartTime,
            request.EndTime,
            excludeReservationId: 0,
            cancellationToken);

        if (overlap)
        {
            return ServiceResult<long>.Conflict(
                $"'{entity.RoomName}' odası seçilen saat aralığında zaten dolu.");
        }

        entity.StartTime = request.StartTime;
        entity.EndTime = request.EndTime;
        entity.Title = request.Title;
        entity.Description = request.Description;
        entity.Status = "pending";

        var reservationId = await _reservationRepository.CreateAsync(entity, cancellationToken);
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

        var buildResult = await BuildReservationAsync(request, cancellationToken);
        if (buildResult.Error is not null)
        {
            return ServiceResult.BadRequest(buildResult.Error.Message!);
        }

        var mapped = buildResult.Entity!;

        if (request.StartTime >= request.EndTime)
        {
            return ServiceResult.Conflict(
                "Başlangıç zamanı, bitiş zamanından önce olmalıdır.");
        }

        var overlap = await HasOverlapAsync(
            mapped.MeetingRoomId,
            mapped.RoomName,
            request.StartTime,
            request.EndTime,
            request.ReservationId,
            cancellationToken);

        if (overlap)
        {
            return ServiceResult.Conflict(
                $"'{mapped.RoomName}' odası seçilen saat aralığında zaten dolu.");
        }

        entity.CompanyId = mapped.CompanyId;
        entity.MeetingRoomId = mapped.MeetingRoomId;
        entity.RoomName = mapped.RoomName;
        entity.OrganizerUserId = mapped.OrganizerUserId;
        entity.RequesterUserId = mapped.RequesterUserId;
        entity.RequesterName = mapped.RequesterName;
        entity.Title = request.Title;
        entity.Description = request.Description;
        entity.StartTime = request.StartTime;
        entity.EndTime = request.EndTime;
        entity.Status = request.Status;

        await _reservationRepository.UpdateAsync(entity, cancellationToken);
        return ServiceResult.NoContent();
    }

    public async Task<ServiceResult> UpdateStatusAsync(
        long id,
        string status,
        CancellationToken cancellationToken = default)
    {
        var entity = await _reservationRepository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return ServiceResult.NotFound($"ID değeri {id} olan rezervasyon bulunamadı.");
        }

        await _reservationRepository.UpdateStatusAsync(id, status, cancellationToken);
        return ServiceResult.NoContent();
    }

    public async Task<ServiceResult> DeleteAsync(
        IdRequest request,
        CancellationToken cancellationToken = default)
    {
        var rowsAffected = await _reservationRepository.DeleteAsync(request.Id, cancellationToken);
        if (rowsAffected == 0)
        {
            return ServiceResult.NotFound(
                $"ID değeri {request.Id} olan rezervasyon bulunamadı.");
        }

        return ServiceResult.NoContent();
    }

    private async Task<(Reservation? Entity, ServiceResult<long>? Error)> BuildReservationAsync(
        CreateReservationDto request,
        CancellationToken cancellationToken)
    {
        var requesterResult = ResolveRequester(request.RequesterUserId, request.RequesterName);
        if (requesterResult.Error is not null)
        {
            return (null, requesterResult.Error);
        }

        MeetingRooms? room = null;
        if (request.MeetingRoomId.HasValue)
        {
            room = await _meetingRoomRepository.GetByIdAsync(
                request.MeetingRoomId.Value,
                cancellationToken);

            if (room is null)
            {
                return (null, ServiceResult<long>.BadRequest("Geçersiz toplantı odası ID değeri."));
            }

            _companyContext.EnsureCanAccessCompany(room.CompanyId);
        }

        var companyId = request.CompanyId ?? room?.CompanyId;
        if (companyId.HasValue)
        {
            _companyContext.EnsureCanAccessCompany(companyId.Value);
        }

        var roomName = !string.IsNullOrWhiteSpace(request.RoomName)
            ? request.RoomName.Trim()
            : room?.Name;

        if (string.IsNullOrWhiteSpace(roomName))
        {
            return (null, ServiceResult<long>.BadRequest("Oda adı veya meetingRoomId zorunludur."));
        }

        return (new Reservation
        {
            CompanyId = companyId,
            MeetingRoomId = request.MeetingRoomId,
            RoomName = roomName,
            OrganizerUserId = request.OrganizerUserId ?? requesterResult.RequesterUserId,
            RequesterUserId = requesterResult.RequesterUserId,
            RequesterName = requesterResult.RequesterName
        }, null);
    }

    private async Task<(Reservation? Entity, ServiceResult<long>? Error)> BuildReservationAsync(
        UpdateReservationDto request,
        CancellationToken cancellationToken)
    {
        var createLike = new CreateReservationDto
        {
            CompanyId = request.CompanyId,
            MeetingRoomId = request.MeetingRoomId,
            RoomName = request.RoomName,
            OrganizerUserId = request.OrganizerUserId,
            RequesterUserId = request.RequesterUserId,
            RequesterName = request.RequesterName,
            Title = request.Title,
            Description = request.Description,
            StartTime = request.StartTime,
            EndTime = request.EndTime
        };

        return await BuildReservationAsync(createLike, cancellationToken);
    }

    private static (int? RequesterUserId, string? RequesterName, ServiceResult<long>? Error) ResolveRequester(
        int? requesterUserId,
        string? requesterName)
    {
        var hasUserId = requesterUserId.HasValue && requesterUserId.Value > 0;
        var hasName = !string.IsNullOrWhiteSpace(requesterName);

        if (!hasUserId && !hasName)
        {
            return (null, null, ServiceResult<long>.BadRequest(
                "requesterUserId veya requesterName zorunludur."));
        }

        if (hasUserId)
        {
            return (requesterUserId, null, null);
        }

        return (null, requesterName!.Trim(), null);
    }

    private async Task<bool> HasOverlapAsync(
        int? meetingRoomId,
        string roomName,
        DateTime startTime,
        DateTime endTime,
        long excludeReservationId,
        CancellationToken cancellationToken)
    {
        if (meetingRoomId.HasValue)
        {
            var count = await _reservationRepository.CountOverlapByMeetingRoomAsync(
                meetingRoomId.Value,
                startTime,
                endTime,
                excludeReservationId,
                cancellationToken);

            return count > 0;
        }

        var nameCount = await _reservationRepository.CountOverlapByRoomNameAsync(
            roomName,
            startTime,
            endTime,
            excludeReservationId,
            cancellationToken);

        return nameCount > 0;
    }

    private static ReservationDto ToDto(Reservation entity) => new()
    {
        ReservationId = entity.ReservationId,
        CompanyId = entity.CompanyId,
        MeetingRoomId = entity.MeetingRoomId,
        RoomName = entity.RoomName,
        OrganizerUserId = entity.OrganizerUserId,
        RequesterUserId = entity.RequesterUserId,
        RequesterName = entity.RequesterName,
        Title = entity.Title,
        Description = entity.Description,
        StartTime = entity.StartTime,
        EndTime = entity.EndTime,
        Status = entity.Status,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };
}
