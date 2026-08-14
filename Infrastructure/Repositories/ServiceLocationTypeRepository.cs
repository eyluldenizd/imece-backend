using Infrastructure.Database.DataAccess;
using Infrastructure.Entities;
using Infrastructure.Repositories.Queries;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Repositories;

public sealed class ServiceLocationTypeRepository
{
    private readonly ISqlDataAccess _dataAccess;

    public ServiceLocationTypeRepository(ISqlDataAccess dataAccess)
    {
        _dataAccess = dataAccess;
    }

    public Task<List<ServiceLocationTypes>> GetAllAsync(
        CompanyListFilter filter,
        CancellationToken cancellationToken = default)
        => _dataAccess.QueryAsync<ServiceLocationTypes>(
            ServiceLocationTypeQueries.GetAll,
            CompanyListFilterParameters.Create(filter),
            cancellationToken);

    public Task<ServiceLocationTypes?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var parameters = new List<SqlParameter> { new("@ServiceLocationTypeId", id) };
        return _dataAccess.QueryFirstOrDefaultAsync<ServiceLocationTypes>(
            ServiceLocationTypeQueries.GetById,
            parameters,
            cancellationToken);
    }

    public Task<ServiceLocationTypes?> GetByNameInCompanyAsync(
        string name,
        int? companyId,
        CancellationToken cancellationToken = default)
    {
        var parameters = new List<SqlParameter>
        {
            new("@Name", name),
            new("@CompanyId", (object?)companyId ?? DBNull.Value)
        };
        return _dataAccess.QueryFirstOrDefaultAsync<ServiceLocationTypes>(
            ServiceLocationTypeQueries.GetByNameInCompany,
            parameters,
            cancellationToken);
    }

    public Task<int> CreateAsync(ServiceLocationTypes entity, CancellationToken cancellationToken = default)
        => _dataAccess.ExecuteScalarAsync<int>(
            ServiceLocationTypeQueries.Create,
            GetWriteParameters(entity, includeId: false),
            cancellationToken);

    public Task<int> UpdateAsync(ServiceLocationTypes entity, CancellationToken cancellationToken = default)
        => _dataAccess.ExecuteAsync(
            ServiceLocationTypeQueries.Update,
            GetWriteParameters(entity, includeId: true),
            cancellationToken);

    public Task<int> SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var parameters = new List<SqlParameter> { new("@ServiceLocationTypeId", id) };
        return _dataAccess.ExecuteAsync(
            ServiceLocationTypeQueries.SoftDelete,
            parameters,
            cancellationToken);
    }

    public Task<int> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var parameters = new List<SqlParameter> { new("@ServiceLocationTypeId", id) };
        return _dataAccess.ExecuteAsync(
            ServiceLocationTypeQueries.Delete,
            parameters,
            cancellationToken);
    }

    private static List<SqlParameter> GetWriteParameters(ServiceLocationTypes entity, bool includeId)
    {
        var parameters = new List<SqlParameter>();

        if (includeId)
        {
            parameters.Add(new SqlParameter("@ServiceLocationTypeId", entity.ServiceLocationTypeId));
        }

        parameters.Add(new SqlParameter("@CompanyScope", entity.CompanyScope));
        parameters.Add(new SqlParameter("@CompanyId", (object?)entity.CompanyId ?? DBNull.Value));
        parameters.Add(new SqlParameter("@BranchScope", entity.BranchScope));
        parameters.Add(new SqlParameter("@BranchId", (object?)entity.BranchId ?? DBNull.Value));
        parameters.Add(new SqlParameter("@DepartmentScope", entity.DepartmentScope));
        parameters.Add(new SqlParameter("@DepartmentId", (object?)entity.DepartmentId ?? DBNull.Value));
        parameters.Add(new SqlParameter("@Name", entity.Name));
        parameters.Add(new SqlParameter("@Description", (object?)entity.Description ?? DBNull.Value));
        parameters.Add(new SqlParameter("@IconUrl", (object?)entity.IconUrl ?? DBNull.Value));
        parameters.Add(new SqlParameter("@ColorKey", (object?)entity.ColorKey ?? DBNull.Value));
        parameters.Add(new SqlParameter("@SortOrder", entity.SortOrder));
        parameters.Add(new SqlParameter("@IsActive", entity.IsActive));

        return parameters;
    }
}
