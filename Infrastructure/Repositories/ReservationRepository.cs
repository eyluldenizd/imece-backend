using Infrastructure.Database.DataAccess;
using Infrastructure.Entities;
using Infrastructure.Repositories.Queries;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Infrastructure.Repositories;

public sealed class ReservationRepository
{
    private readonly ISqlDataAccess _dataAccess;

    public ReservationRepository(ISqlDataAccess dataAccess)
    {
        _dataAccess = dataAccess;
    }

    public Task<List<Reservation>> GetAllAsync(CancellationToken cancellationToken = default)
        => _dataAccess.QueryAsync<Reservation>(ReservationsQueries.GetAll, cancellationToken: cancellationToken);

    public Task<Reservation?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        SqlParameter[] parameters =
        [
            new("@ReservationId", SqlDbType.Int) { Value = (int)id }
        ];

        return _dataAccess.QueryFirstOrDefaultAsync<Reservation>(
            ReservationsQueries.GetById,
            parameters,
            cancellationToken);
    }

    public Task<List<Reservation>> GetByOrganizerAsync(
        long organizerUserId,
        CancellationToken cancellationToken = default)
    {
        SqlParameter[] parameters =
        [
            new("@OrganizerUserId", SqlDbType.Int) { Value = (int)organizerUserId }
        ];

        return _dataAccess.QueryAsync<Reservation>(
            ReservationsQueries.GetByOrganizer,
            parameters,
            cancellationToken: cancellationToken);
    }

    public Task<List<Reservation>> GetByRoomNameAsync(
        string roomName,
        CancellationToken cancellationToken = default)
    {
        SqlParameter[] parameters =
        [
            new("@RoomName", roomName)
        ];

        return _dataAccess.QueryAsync<Reservation>(
            ReservationsQueries.GetByRoomName,
            parameters,
            cancellationToken: cancellationToken);
    }

    public async Task<int> CountOverlapByRoomNameAsync(
        string roomName,
        DateTime startTime,
        DateTime endTime,
        long excludeReservationId,
        CancellationToken cancellationToken = default)
    {
        SqlParameter[] parameters =
        [
            new("@RoomName", roomName),
            new("@StartTime", startTime),
            new("@EndTime", endTime),
            new("@ExcludeReservationId", SqlDbType.Int) { Value = (int)excludeReservationId }
        ];

        var rows = await _dataAccess.QueryAsync<int>(
            ReservationsQueries.CheckOverlapByRoomName,
            parameters,
            cancellationToken: cancellationToken);

        return rows.Count;
    }

    public async Task<int> CountOverlapByMeetingRoomAsync(
        int meetingRoomId,
        DateTime startTime,
        DateTime endTime,
        long excludeReservationId,
        CancellationToken cancellationToken = default)
    {
        SqlParameter[] parameters =
        [
            new("@MeetingRoomId", meetingRoomId),
            new("@StartTime", startTime),
            new("@EndTime", endTime),
            new("@ExcludeReservationId", SqlDbType.Int) { Value = (int)excludeReservationId }
        ];

        var rows = await _dataAccess.QueryAsync<int>(
            ReservationsQueries.CheckOverlapByMeetingRoom,
            parameters,
            cancellationToken: cancellationToken);

        return rows.Count;
    }

    public async Task<long> CreateAsync(
        Reservation reservation,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var parameters = BuildWriteParameters(reservation, now);
        return await _dataAccess.ExecuteScalarAsync<long>(
            ReservationsQueries.Create,
            parameters,
            cancellationToken);
    }

    public Task<int> UpdateAsync(
        Reservation reservation,
        CancellationToken cancellationToken = default)
        => _dataAccess.ExecuteAsync(
            ReservationsQueries.Update,
            BuildWriteParameters(reservation, reservation.UpdatedAt, includeReservationId: true),
            cancellationToken);

    public Task<int> UpdateStatusAsync(
        long id,
        string status,
        CancellationToken cancellationToken = default)
    {
        SqlParameter[] parameters =
        [
            new("@ReservationId", SqlDbType.Int) { Value = (int)id },
            new("@Status", status)
        ];

        return _dataAccess.ExecuteAsync(ReservationsQueries.UpdateStatus, parameters, cancellationToken);
    }

    public Task<int> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        SqlParameter[] parameters =
        [
            new("@ReservationId", SqlDbType.Int) { Value = (int)id }
        ];

        return _dataAccess.ExecuteAsync(ReservationsQueries.Delete, parameters, cancellationToken);
    }

    private static SqlParameter[] BuildWriteParameters(
        Reservation reservation,
        DateTime timestamp,
        bool includeReservationId = false)
    {
        var list = new List<SqlParameter>();

        if (includeReservationId)
        {
            list.Add(new("@ReservationId", reservation.ReservationId));
        }

        list.Add(new("@CompanyId", (object?)reservation.CompanyId ?? DBNull.Value));
        list.Add(new("@MeetingRoomId", (object?)reservation.MeetingRoomId ?? DBNull.Value));
        list.Add(new("@RoomName", reservation.RoomName));
        list.Add(new("@OrganizerUserId", (object?)reservation.OrganizerUserId ?? DBNull.Value));
        list.Add(new("@RequesterUserId", (object?)reservation.RequesterUserId ?? DBNull.Value));
        list.Add(new("@RequesterName", (object?)reservation.RequesterName ?? DBNull.Value));
        list.Add(new("@Title", reservation.Title));
        list.Add(new("@Description", (object?)reservation.Description ?? DBNull.Value));
        list.Add(new("@StartTime", reservation.StartTime));
        list.Add(new("@EndTime", reservation.EndTime));
        list.Add(new("@Status", reservation.Status));

        if (!includeReservationId)
        {
            list.Add(new("@CreatedAt", timestamp));
            list.Add(new("@UpdatedAt", timestamp));
        }

        return list.ToArray();
    }
}
