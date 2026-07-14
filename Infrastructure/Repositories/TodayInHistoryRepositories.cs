
using Infrastructure.Data;
using Infrastructure.Queries;
using Microsoft.Data.SqlClient;
using System.Data;
using Infrastructure.Entities;

namespace Infrastructure.Repositories;

public sealed class TodayInHistoryRepository
{
    private readonly DbManager _dbManager;

    public TodayInHistoryRepository(DbManager dbManager)
    {
        _dbManager = dbManager;
    }


   public Task<List<TodayInHistory>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return _dbManager.QueryAsync(
            TodayInHistoryQueries.GetAll,
            reader => new TodayInHistory
            {
                Id = reader.GetInt64(
                    reader.GetOrdinal("id")),

                EventDate = reader.GetDateTime(
                    reader.GetOrdinal("event_date")),

                Title = reader.GetString(
                    reader.GetOrdinal("title")),

                Description = reader.IsDBNull(
                    reader.GetOrdinal("description"))
                    ? null
                    : reader.GetString(
                        reader.GetOrdinal("description")),

                ImageUrl = reader.IsDBNull(
                    reader.GetOrdinal("image_url"))
                    ? null
                    : reader.GetString(
                        reader.GetOrdinal("image_url")),

                CreatedAt = reader.GetDateTime(
                    reader.GetOrdinal("created_at"))
            },
            cancellationToken: cancellationToken);
    }


    public Task<int> CreateAsync(
        TodayInHistory today,
        CancellationToken cancellationToken = default)
    {
        var parameters = new[]
        {
            new SqlParameter(
                "@EventDate",
                SqlDbType.Date)
            {
                Value = today.EventDate
            },

            new SqlParameter(
                "@Title",
                today.Title),

            new SqlParameter(
                "@Description",
                (object?)today.Description ?? DBNull.Value),

            new SqlParameter(
                "@ImageUrl",
                (object?)today.ImageUrl ?? DBNull.Value)
        };


        return _dbManager.ExecuteAsync(
            TodayInHistoryQueries.Create,
            parameters,
            cancellationToken);
    }


    public Task<int> UpdateAsync(
        TodayInHistory today,
        CancellationToken cancellationToken = default)
    {
        var parameters = new[]
        {
            new SqlParameter(
                "@Id",
                SqlDbType.BigInt)
            {
                Value = today.Id
            },

            new SqlParameter(
                "@EventDate",
                SqlDbType.Date)
            {
                Value = today.EventDate
            },

            new SqlParameter(
                "@Title",
                today.Title),

            new SqlParameter(
                "@Description",
                (object?)today.Description ?? DBNull.Value),

            new SqlParameter(
                "@ImageUrl",
                (object?)today.ImageUrl ?? DBNull.Value)
        };


        return _dbManager.ExecuteAsync(
            TodayInHistoryQueries.Update,
            parameters,
            cancellationToken);
    }


    public Task<int> DeleteAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        var parameters = new[]
        {
            new SqlParameter(
                "@Id",
                SqlDbType.BigInt)
            {
                Value = id
            }
        };


        return _dbManager.ExecuteAsync(
            TodayInHistoryQueries.Delete,
            parameters,
            cancellationToken);
    }
}