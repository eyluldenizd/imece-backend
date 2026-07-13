using System.Data;
using Infrastructure.Data;
using Infrastructure.Entities;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Repositories;

public sealed class EventRepository
{
    private readonly DbManager _dbManager;

    private const string SelectColumns = """
        SELECT
            event_id,
            title,
            description,
            event_type,
            location,
            cover_image_url,
            start_datetime,
            end_datetime,
            is_all_day,
            created_by,
            created_at
        FROM events
        """;

    public EventRepository(
        DbManager dbManager)
    {
        _dbManager = dbManager;
    }

    public Task<List<Events>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var sql = $"""
            {SelectColumns}
            ORDER BY start_datetime ASC;
            """;

        return _dbManager.QueryAsync<Events>(
            sql,
            cancellationToken: cancellationToken);
    }

    public Task<List<Events>> GetUpcomingAsync(
        CancellationToken cancellationToken = default)
    {
        var sql = $"""
            {SelectColumns}
            WHERE end_datetime >= SYSDATETIME()
            ORDER BY start_datetime ASC;
            """;

        return _dbManager.QueryAsync<Events>(
            sql,
            cancellationToken: cancellationToken);
    }

    public Task<Events?> GetByIdAsync(
        long eventId,
        CancellationToken cancellationToken = default)
    {
        var sql = $"""
            {SelectColumns}
            WHERE event_id = @EventId;
            """;

        SqlParameter[] parameters =
        [
            new SqlParameter(
                "@EventId",
                SqlDbType.BigInt)
            {
                Value = eventId
            }
        ];

        return _dbManager.QueryFirstOrDefaultAsync<Events>(
            sql,
            parameters,
            cancellationToken);
    }

    public async Task<long> CreateAsync(
        Events entity,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO events
            (
                title,
                description,
                event_type,
                location,
                cover_image_url,
                start_datetime,
                end_datetime,
                is_all_day,
                created_by
            )
            OUTPUT INSERTED.event_id
            VALUES
            (
                @Title,
                @Description,
                @EventType,
                @Location,
                @CoverImageUrl,
                @StartDateTime,
                @EndDateTime,
                @IsAllDay,
                @CreatedBy
            );
            """;

        var parameters = CreateWriteParameters(entity);

        return await _dbManager.ExecuteScalarAsync<long>(
            sql,
            parameters,
            cancellationToken);
    }

    public Task<int> UpdateAsync(
        Events entity,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE events
            SET
                title = @Title,
                description = @Description,
                event_type = @EventType,
                location = @Location,
                cover_image_url = @CoverImageUrl,
                start_datetime = @StartDateTime,
                end_datetime = @EndDateTime,
                is_all_day = @IsAllDay,
                created_by = @CreatedBy
            WHERE event_id = @EventId;
            """;

        var parameters = CreateWriteParameters(entity)
            .ToList();

        parameters.Add(
            new SqlParameter(
                "@EventId",
                SqlDbType.BigInt)
            {
                Value = entity.EventId
            });

        return _dbManager.ExecuteAsync(
            sql,
            parameters,
            cancellationToken);
    }

    public Task<int> DeleteAsync(
        long eventId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            DELETE FROM events
            WHERE event_id = @EventId;
            """;

        SqlParameter[] parameters =
        [
            new SqlParameter(
                "@EventId",
                SqlDbType.BigInt)
            {
                Value = eventId
            }
        ];

        return _dbManager.ExecuteAsync(
            sql,
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