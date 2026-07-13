using Infrastructure.Data;
using Infrastructure.Entities;
using Infrastructure.Repositories.Queries;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Infrastructure.Repositories;

public sealed class DishesRepository
{
    private readonly DbManager _dbManager;

    public DishesRepository(DbManager dbManager)
    {
        _dbManager = dbManager;
    }

    public Task<List<Dishes>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return _dbManager.QueryAsync<Dishes>(
            DishesQueries.GetAll,
            cancellationToken: cancellationToken);
    }

    public Task<Dishes?> GetByIdAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        var parameters = new[]
        {
            new SqlParameter("@DishId", SqlDbType.BigInt)
            {
                Value = id
            }
        };

        return _dbManager.QueryFirstOrDefaultAsync<Dishes>(
            DishesQueries.GetById,
            parameters,
            cancellationToken);
    }

    public Task<int> DeleteAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        var parameters = new[]
        {
            new SqlParameter("@DishId", SqlDbType.BigInt)
            {
                Value = id
            }
        };

        return _dbManager.ExecuteAsync(
            DishesQueries.Delete,
            parameters,
            cancellationToken);
    }
}