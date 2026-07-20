using Infrastructure.Database.DataAccess;
using Infrastructure.Entities;
using Infrastructure.Repositories.Queries;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Repositories;

public sealed class DishesRepository
{
    private readonly ISqlDataAccess _dataAccess;

    public DishesRepository(ISqlDataAccess dataAccess)
    {
        _dataAccess = dataAccess;
    }

    public Task<List<Dishes>> GetAllAsync(CancellationToken cancellationToken = default) =>
        _dataAccess.QueryAsync<Dishes>(DishesQueries.GetAll, cancellationToken: cancellationToken);

    public Task<List<Dishes>> GetActiveAsync(CancellationToken cancellationToken = default) =>
        _dataAccess.QueryAsync<Dishes>(DishesQueries.GetActive, cancellationToken: cancellationToken);

    public Task<Dishes?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var parameters = new List<SqlParameter> { new("@DishId", id) };
        return _dataAccess.QueryFirstOrDefaultAsync<Dishes>(
            DishesQueries.GetById,
            parameters,
            cancellationToken);
    }

    public Task<int> CreateAsync(Dishes entity, CancellationToken cancellationToken = default)
    {
        var parameters = GetWriteParameters(entity, includeId: false);
        return _dataAccess.ExecuteScalarAsync<int>(DishesQueries.Create, parameters, cancellationToken);
    }

    public Task<int> UpdateAsync(Dishes entity, CancellationToken cancellationToken = default)
    {
        var parameters = GetWriteParameters(entity, includeId: true);
        return _dataAccess.ExecuteAsync(DishesQueries.Update, parameters, cancellationToken);
    }

    public Task<int> SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var parameters = new List<SqlParameter> { new("@DishId", id) };
        return _dataAccess.ExecuteAsync(DishesQueries.SoftDelete, parameters, cancellationToken);
    }

    private static List<SqlParameter> GetWriteParameters(Dishes entity, bool includeId)
    {
        var parameters = new List<SqlParameter>();

        if (includeId)
        {
            parameters.Add(new SqlParameter("@DishId", entity.DishId));
        }

        parameters.Add(new SqlParameter("@DishName", entity.DishName));
        parameters.Add(new SqlParameter("@DishCategoryId", (object?)entity.DishCategoryId ?? DBNull.Value));
        parameters.Add(new SqlParameter("@Category", entity.Category));
        parameters.Add(new SqlParameter("@Description", (object?)entity.Description ?? DBNull.Value));
        parameters.Add(new SqlParameter("@ImageUrl", (object?)entity.ImageUrl ?? DBNull.Value));
        parameters.Add(new SqlParameter("@IsActive", entity.IsActive));

        return parameters;
    }
}
