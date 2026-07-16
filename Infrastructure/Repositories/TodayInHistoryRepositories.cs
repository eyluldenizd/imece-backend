
using Infrastructure.Data;
using Infrastructure.Entities;
using Infrastructure.Queries;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Infrastructure.Repositories;

public sealed class TodayInHistoryRepository
{
    private readonly DbManager _dbManager;

    public TodayInHistoryRepository(DbManager dbManager)
    {
        _dbManager = dbManager;
    }

    public Task<List<TodayInHistory>> GetAllAsync(CancellationToken cancellationToken = default)
        => _dbManager.QueryAsync<TodayInHistory>(TodayInHistoryQueries.GetAll, null, cancellationToken);

    public Task<int> CreateAsync(TodayInHistory today, CancellationToken cancellationToken = default)
    {
        SqlParameter[] parameters =
        [
            new SqlParameter("@EventDate", SqlDbType.Date) { Value = today.EventDate },
            new SqlParameter("@Title", today.Title),
            new SqlParameter("@Description", (object?)today.Description ?? DBNull.Value),
            new SqlParameter("@ImageUrl", (object?)today.ImageUrl ?? DBNull.Value),
        ];

        return _dbManager.ExecuteAsync(TodayInHistoryQueries.Create, parameters, cancellationToken);
    }

    public Task<int> UpdateAsync(TodayInHistory today, CancellationToken cancellationToken = default)
    {
        SqlParameter[] parameters =
        [
            new SqlParameter("@Id", SqlDbType.BigInt) { Value = today.Id },
            new SqlParameter("@EventDate", SqlDbType.Date) { Value = today.EventDate },
            new SqlParameter("@Title", today.Title),
            new SqlParameter("@Description", (object?)today.Description ?? DBNull.Value),
            new SqlParameter("@ImageUrl", (object?)today.ImageUrl ?? DBNull.Value),
        ];

        return _dbManager.ExecuteAsync(TodayInHistoryQueries.Update, parameters, cancellationToken);
    }

    public Task<int> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        SqlParameter[] parameters =
        [
            new SqlParameter("@Id", SqlDbType.BigInt) { Value = id }
        ];

        return _dbManager.ExecuteAsync(TodayInHistoryQueries.Delete, parameters, cancellationToken);
    }
}
