using Infrastructure.Database.DataAccess;
using Infrastructure.Entities;
using Infrastructure.Repositories.Queries;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Infrastructure.Repositories;

public sealed class DishesRepository
{
    private readonly ISqlDataAccess _dataAccess;

    public DishesRepository(ISqlDataAccess dataAccess)
    {
        _dataAccess = dataAccess;
    }

    public Task<List<Dishes>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return _dataAccess.QueryAsync<Dishes>(
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

        return _dataAccess.QueryFirstOrDefaultAsync<Dishes>(
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

        return _dataAccess.ExecuteAsync(
            DishesQueries.Delete,
            parameters,
            cancellationToken);
    }
}