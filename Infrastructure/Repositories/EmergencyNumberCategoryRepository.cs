using Infrastructure.Database.DataAccess;
using Infrastructure.Entities;
using Infrastructure.Repositories.Queries;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Repositories;

public sealed class EmergencyNumberCategoryRepository
{
    private readonly ISqlDataAccess _dataAccess;

    public EmergencyNumberCategoryRepository(ISqlDataAccess dataAccess)
    {
        _dataAccess = dataAccess;
    }

    public Task<List<EmergencyNumberCategories>> GetAllAsync(CancellationToken cancellationToken = default)
        => _dataAccess.QueryAsync<EmergencyNumberCategories>(
            EmergencyNumberCategoryQueries.GetAll,
            null,
            cancellationToken);

    public Task<EmergencyNumberCategories?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var parameters = new List<SqlParameter> { new("@EmergencyNumberCategoryId", id) };
        return _dataAccess.QueryFirstOrDefaultAsync<EmergencyNumberCategories>(
            EmergencyNumberCategoryQueries.GetById,
            parameters,
            cancellationToken);
    }

    public Task<EmergencyNumberCategories?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var parameters = new List<SqlParameter> { new("@Name", name) };
        return _dataAccess.QueryFirstOrDefaultAsync<EmergencyNumberCategories>(
            EmergencyNumberCategoryQueries.GetByName,
            parameters,
            cancellationToken);
    }

    public Task<int> CreateAsync(EmergencyNumberCategories entity, CancellationToken cancellationToken = default)
        => _dataAccess.ExecuteScalarAsync<int>(
            EmergencyNumberCategoryQueries.Create,
            GetWriteParameters(entity, includeId: false),
            cancellationToken);

    public Task<int> UpdateAsync(EmergencyNumberCategories entity, CancellationToken cancellationToken = default)
        => _dataAccess.ExecuteAsync(
            EmergencyNumberCategoryQueries.Update,
            GetWriteParameters(entity, includeId: true),
            cancellationToken);

    public Task<int> SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var parameters = new List<SqlParameter> { new("@EmergencyNumberCategoryId", id) };
        return _dataAccess.ExecuteAsync(
            EmergencyNumberCategoryQueries.SoftDelete,
            parameters,
            cancellationToken);
    }

    public Task<int> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var parameters = new List<SqlParameter> { new("@EmergencyNumberCategoryId", id) };
        return _dataAccess.ExecuteAsync(
            EmergencyNumberCategoryQueries.Delete,
            parameters,
            cancellationToken);
    }

    private static List<SqlParameter> GetWriteParameters(EmergencyNumberCategories entity, bool includeId)
    {
        var parameters = new List<SqlParameter>();

        if (includeId)
        {
            parameters.Add(new SqlParameter("@EmergencyNumberCategoryId", entity.EmergencyNumberCategoryId));
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
