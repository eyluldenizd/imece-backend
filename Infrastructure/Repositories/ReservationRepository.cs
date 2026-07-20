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

    public Task<List<Reservation>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return _dataAccess.QueryAsync<Reservation>(
            ReservationsQueries.GetAll,
            cancellationToken: cancellationToken);
    }

    public Task<Reservation?> GetByIdAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        var parameters = new[]
        {
            new SqlParameter("@ReservationId", SqlDbType.BigInt) { Value = id }
        };

        return _dataAccess.QueryFirstOrDefaultAsync<Reservation>(
            ReservationsQueries.GetById,
            parameters,
            cancellationToken);
    }

    public Task<List<Reservation>> GetByOrganizerAsync(
        long organizerUserId,
        CancellationToken cancellationToken = default)
    {
        var parameters = new[]
        {
            new SqlParameter("@OrganizerUserId", SqlDbType.BigInt) { Value = organizerUserId }
        };

        return _dataAccess.QueryAsync<Reservation>(
            ReservationsQueries.GetByOrganizer,
            parameters,
            cancellationToken: cancellationToken);
    }

    public Task<List<Reservation>> GetByRoomNameAsync(
        string roomName,
        CancellationToken cancellationToken = default)
    {
        var parameters = new[]
        {
            new SqlParameter("@RoomName", roomName)
        };

        return _dataAccess.QueryAsync<Reservation>(
            ReservationsQueries.GetByRoomName,
            parameters,
            cancellationToken: cancellationToken);
    }

    public Task<List<Reservation>> CheckOverlapAsync(
        string roomName,
        DateTime startTime,
        DateTime endTime,
        long excludeReservationId,
        CancellationToken cancellationToken = default)
    {
        var parameters = new[]
        {
            new SqlParameter("@RoomName", roomName),
            new SqlParameter("@StartTime", startTime),
            new SqlParameter("@EndTime", endTime),
            new SqlParameter("@ExcludeReservationId", SqlDbType.BigInt) { Value = excludeReservationId }
        };

        return _dataAccess.QueryAsync<Reservation>(
            ReservationsQueries.CheckOverlap,
            parameters,
            cancellationToken: cancellationToken);
    }

    public async Task<long> CreateAsync(
        Reservation reservation,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        var parameters = new[]
        {
            new SqlParameter("@RoomName", reservation.RoomName),
            new SqlParameter("@OrganizerUserId", reservation.OrganizerUserId),
            new SqlParameter("@Title", reservation.Title),
            new SqlParameter("@Description", (object?)reservation.Description ?? DBNull.Value),
            new SqlParameter("@StartTime", reservation.StartTime),
            new SqlParameter("@EndTime", reservation.EndTime),
            new SqlParameter("@Status", reservation.Status),
            new SqlParameter("@CreatedAt", now),
            new SqlParameter("@UpdatedAt", now)
        };

        var newId = await _dataAccess.ExecuteScalarAsync<long>(
            ReservationsQueries.Create,
            parameters,
            cancellationToken);

        return newId;
    }

    public Task<int> UpdateAsync(
        Reservation reservation,
        CancellationToken cancellationToken = default)
    {
        var parameters = new[]
        {
            new SqlParameter("@ReservationId", reservation.ReservationId),
            new SqlParameter("@RoomName", reservation.RoomName),
            new SqlParameter("@OrganizerUserId", reservation.OrganizerUserId),
            new SqlParameter("@Title", reservation.Title),
            new SqlParameter("@Description", (object?)reservation.Description ?? DBNull.Value),
            new SqlParameter("@StartTime", reservation.StartTime),
            new SqlParameter("@EndTime", reservation.EndTime),
            new SqlParameter("@Status", reservation.Status)
        };

        return _dataAccess.ExecuteAsync(
            ReservationsQueries.Update,
            parameters,
            cancellationToken);
    }

    public Task<int> UpdateStatusAsync(
        long id,
        string status,
        CancellationToken cancellationToken = default)
    {
        var parameters = new[]
        {
            new SqlParameter("@ReservationId", SqlDbType.BigInt) { Value = id },
            new SqlParameter("@Status", status)
        };

        return _dataAccess.ExecuteAsync(
            ReservationsQueries.UpdateStatus,
            parameters,
            cancellationToken);
    }

    public Task<int> DeleteAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        var parameters = new[]
        {
            new SqlParameter("@ReservationId", SqlDbType.BigInt) { Value = id }
        };

        return _dataAccess.ExecuteAsync(
            ReservationsQueries.Delete,
            parameters,
            cancellationToken);
    }
}