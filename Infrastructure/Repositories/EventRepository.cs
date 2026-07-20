using System.Data;
using Infrastructure.Database.DataAccess;
using Infrastructure.Entities;
using Infrastructure.Repositories.Queries;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Repositories;

public sealed class EventRepository
{
    private readonly ISqlDataAccess _dataAccess;

    public EventRepository(
        ISqlDataAccess dataAccess)
    {
        _dataAccess = dataAccess;
    }

    public Task<List<Events>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return _dataAccess.QueryAsync<Events>(
            EventQueries.GetAll,
            cancellationToken: cancellationToken);
    }

    public Task<List<Events>> GetUpcomingAsync(
        CancellationToken cancellationToken = default)
    {
        return _dataAccess.QueryAsync<Events>(
            EventQueries.GetUpcoming,
            cancellationToken: cancellationToken);
    }

    public Task<Events?> GetByIdAsync(
        long eventId,
        CancellationToken cancellationToken = default)
    {
        SqlParameter[] parameters =
        [
            new SqlParameter(
                "@EventId",
                SqlDbType.BigInt)
            {
                Value = eventId
            }
        ];

        return _dataAccess.QueryFirstOrDefaultAsync<Events>(
            EventQueries.GetById,
            parameters,
            cancellationToken);
    }

    public Task<long> CreateAsync(
        Events entity,
        CancellationToken cancellationToken = default)
    {
        var parameters = CreateWriteParameters(entity);

        return _dataAccess.ExecuteScalarAsync<long>(
            EventQueries.Create,
            parameters,
            cancellationToken);
    }

    public Task<int> UpdateAsync(
        Events entity,
        CancellationToken cancellationToken = default)
    {
        var parameters = CreateWriteParameters(entity)
            .ToList();

        parameters.Add(
            new SqlParameter(
                "@EventId",
                SqlDbType.BigInt)
            {
                Value = entity.EventId
            });

        return _dataAccess.ExecuteAsync(
            EventQueries.Update,
            parameters,
            cancellationToken);
    }

    public Task<int> DeleteAsync(
        long eventId,
        CancellationToken cancellationToken = default)
    {
        SqlParameter[] parameters =
        [
            new SqlParameter(
                "@EventId",
                SqlDbType.BigInt)
            {
                Value = eventId
            }
        ];

        return _dataAccess.ExecuteAsync(
            EventQueries.Delete,
            parameters,
            cancellationToken);
    }

    private static SqlParameter[] CreateWriteParameters(
        Events entity)
    {
        return
        [
            new SqlParameter(
                "@Title",
                SqlDbType.NVarChar,
                255)
            {
                Value = entity.Title
            },

            new SqlParameter(
                "@Description",
                SqlDbType.NVarChar,
                -1)
            {
                Value = entity.Description
                    ?? (object)DBNull.Value
            },

            new SqlParameter(
                "@EventType",
                SqlDbType.NVarChar,
                50)
            {
                Value = entity.EventType
                    ?? (object)DBNull.Value
            },

            new SqlParameter(
                "@Location",
                SqlDbType.NVarChar,
                255)
            {
                Value = entity.Location
                    ?? (object)DBNull.Value
            },

            new SqlParameter(
                "@CoverImageUrl",
                SqlDbType.NVarChar,
                255)
            {
                Value = entity.CoverImageUrl
                    ?? (object)DBNull.Value
            },

            new SqlParameter(
                "@StartDateTime",
                SqlDbType.DateTime2)
            {
                Value = entity.StartDateTime
            },

            new SqlParameter(
                "@EndDateTime",
                SqlDbType.DateTime2)
            {
                Value = entity.EndDateTime
            },

            new SqlParameter(
                "@IsAllDay",
                SqlDbType.Bit)
            {
                Value = entity.IsAllDay
            },

            new SqlParameter(
                "@CreatedBy",
                SqlDbType.Int)
            {
                Value = entity.CreatedBy
            }
        ];
    }
}