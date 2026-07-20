using Infrastructure.Database.DataAccess;
using Infrastructure.Entities;
using Infrastructure.Repositories.Queries;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Repositories;

public sealed class DishCategoryRepository
{
    private readonly ISqlDataAccess _dataAccess;

    public DishCategoryRepository(ISqlDataAccess dataAccess)
    {
        _dataAccess = dataAccess;
    }

    public Task<List<DishCategories>> GetAllAsync(CancellationToken cancellationToken = default) =>
        _dataAccess.QueryAsync<DishCategories>(DishCategoryQueries.GetAll, null, cancellationToken);

    public Task<List<DishCategories>> GetActiveAsync(CancellationToken cancellationToken = default) =>
        _dataAccess.QueryAsync<DishCategories>(DishCategoryQueries.GetActive, null, cancellationToken);

    public Task<DishCategories?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var parameters = new List<SqlParameter> { new("@DishCategoryId", id) };
        return _dataAccess.QueryFirstOrDefaultAsync<DishCategories>(
            DishCategoryQueries.GetById,
            parameters,
            cancellationToken);
    }

    public Task<DishCategories?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var parameters = new List<SqlParameter> { new("@Code", code) };
        return _dataAccess.QueryFirstOrDefaultAsync<DishCategories>(
            DishCategoryQueries.GetByCode,
            parameters,
            cancellationToken);
    }

    public Task<int> CreateAsync(DishCategories entity, CancellationToken cancellationToken = default)
    {
        var parameters = GetWriteParameters(entity, includeId: false);
        return _dataAccess.ExecuteScalarAsync<int>(DishCategoryQueries.Create, parameters, cancellationToken);
    }

    public Task<int> UpdateAsync(DishCategories entity, CancellationToken cancellationToken = default)
    {
        var parameters = GetWriteParameters(entity, includeId: true);
        return _dataAccess.ExecuteAsync(DishCategoryQueries.Update, parameters, cancellationToken);
    }

    public Task<int> SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var parameters = new List<SqlParameter> { new("@DishCategoryId", id) };
        return _dataAccess.ExecuteAsync(DishCategoryQueries.SoftDelete, parameters, cancellationToken);
    }

    private static List<SqlParameter> GetWriteParameters(DishCategories entity, bool includeId)
    {
        var parameters = new List<SqlParameter>();

        if (includeId)
        {
            parameters.Add(new SqlParameter("@DishCategoryId", entity.DishCategoryId));
        }

        parameters.Add(new SqlParameter("@Name", entity.Name));
        parameters.Add(new SqlParameter("@Code", entity.Code));
        parameters.Add(new SqlParameter("@Description", (object?)entity.Description ?? DBNull.Value));
        parameters.Add(new SqlParameter("@SortOrder", entity.SortOrder));
        parameters.Add(new SqlParameter("@IsActive", entity.IsActive));

        return parameters;
    }
}
