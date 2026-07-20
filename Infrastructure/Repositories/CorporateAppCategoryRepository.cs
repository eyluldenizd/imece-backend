using Infrastructure.Database.DataAccess;
using Infrastructure.Entities;
using Infrastructure.Repositories.Queries;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Repositories;

public sealed class CorporateAppCategoryRepository
{
    private readonly ISqlDataAccess _dataAccess;

    public CorporateAppCategoryRepository(ISqlDataAccess dataAccess)
    {
        _dataAccess = dataAccess;
    }

    public Task<List<CorporateAppCategories>> GetAllAsync(CancellationToken cancellationToken = default)
        => _dataAccess.QueryAsync<CorporateAppCategories>(
            CorporateAppCategoryQueries.GetAll,
            null,
            cancellationToken);

    public Task<CorporateAppCategories?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var parameters = new List<SqlParameter> { new("@CorporateAppCategoryId", id) };
        return _dataAccess.QueryFirstOrDefaultAsync<CorporateAppCategories>(
            CorporateAppCategoryQueries.GetById,
            parameters,
            cancellationToken);
    }

    public Task<CorporateAppCategories?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var parameters = new List<SqlParameter> { new("@Name", name) };
        return _dataAccess.QueryFirstOrDefaultAsync<CorporateAppCategories>(
            CorporateAppCategoryQueries.GetByName,
            parameters,
            cancellationToken);
    }

    public Task<int> CreateAsync(CorporateAppCategories entity, CancellationToken cancellationToken = default)
        => _dataAccess.ExecuteScalarAsync<int>(
            CorporateAppCategoryQueries.Create,
            GetWriteParameters(entity, includeId: false),
            cancellationToken);

    public Task<int> UpdateAsync(CorporateAppCategories entity, CancellationToken cancellationToken = default)
        => _dataAccess.ExecuteAsync(
            CorporateAppCategoryQueries.Update,
            GetWriteParameters(entity, includeId: true),
            cancellationToken);

    public Task<int> SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var parameters = new List<SqlParameter> { new("@CorporateAppCategoryId", id) };
        return _dataAccess.ExecuteAsync(
            CorporateAppCategoryQueries.SoftDelete,
            parameters,
            cancellationToken);
    }

    private static List<SqlParameter> GetWriteParameters(CorporateAppCategories entity, bool includeId)
    {
        var parameters = new List<SqlParameter>();

        if (includeId)
        {
            parameters.Add(new SqlParameter("@CorporateAppCategoryId", entity.CorporateAppCategoryId));
        }

        parameters.Add(new SqlParameter("@Name", entity.Name));
        parameters.Add(new SqlParameter("@Description", (object?)entity.Description ?? DBNull.Value));
        parameters.Add(new SqlParameter("@IconUrl", (object?)entity.IconUrl ?? DBNull.Value));
        parameters.Add(new SqlParameter("@ColorKey", (object?)entity.ColorKey ?? DBNull.Value));
        parameters.Add(new SqlParameter("@SortOrder", entity.SortOrder));
        parameters.Add(new SqlParameter("@IsActive", entity.IsActive));

        return parameters;
    }
}
