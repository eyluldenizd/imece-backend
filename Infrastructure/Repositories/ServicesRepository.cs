using Core.Entities;
using Infrastructure.Database.DataAccess;
using Infrastructure.Repositories.Queries;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Repositories;

public sealed class ServicesRepository
{
    private readonly ISqlDataAccess _dataAccess;

    public ServicesRepository(ISqlDataAccess dataAccess)
    {
        _dataAccess = dataAccess;
    }

    public Task<List<Services>> GetAllAsync(CancellationToken cancellationToken = default) 
        => _dataAccess.QueryAsync<Services>(ServicesQueries.GetAll, null, cancellationToken);

    public Task<List<Services>> GetActiveAsync(CancellationToken cancellationToken = default) 
        => _dataAccess.QueryAsync<Services>(ServicesQueries.GetActive, null, cancellationToken);

    public Task<Services?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var parameters = new List<SqlParameter>
        {
            new("@ServiceId", id)
        };
        return _dataAccess.QueryFirstOrDefaultAsync<Services>(ServicesQueries.GetById, parameters, cancellationToken);
    }

    public Task<long> CreateAsync(Services entity, CancellationToken cancellationToken = default)
    {
        var parameters = GetWriteParameters(entity, includeId: false);
        return _dataAccess.ExecuteScalarAsync<long>(ServicesQueries.Create, parameters, cancellationToken);
    }

    public Task<int> UpdateAsync(Services entity, CancellationToken cancellationToken = default)
    {
        var parameters = GetWriteParameters(entity, includeId: true);
        return _dataAccess.ExecuteAsync(ServicesQueries.Update, parameters, cancellationToken);
    }

    public Task<int> SoftDeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var parameters = new List<SqlParameter>
        {
            new("@ServiceId", id)
        };
        return _dataAccess.ExecuteAsync(ServicesQueries.SoftDelete, parameters, cancellationToken);
    }

    private static List<SqlParameter> GetWriteParameters(Services entity, bool includeId)
    {
        var parameters = new List<SqlParameter>();

        if (includeId)
        {
            parameters.Add(new SqlParameter("@ServiceId", entity.ServiceId));
        }

        parameters.Add(new SqlParameter("@Name", entity.Name));
        parameters.Add(new SqlParameter("@Description", (object?)entity.Description ?? DBNull.Value));
        parameters.Add(new SqlParameter("@Icon", (object?)entity.Icon ?? DBNull.Value));
        parameters.Add(new SqlParameter("@IsActive", entity.IsActive));
        parameters.Add(new SqlParameter("@CompanyScope", entity.CompanyScope));
        parameters.Add(new SqlParameter("@CompanyId", (object?)entity.CompanyId ?? DBNull.Value));
        parameters.Add(new SqlParameter("@BranchScope", entity.BranchScope));
        parameters.Add(new SqlParameter("@BranchId", (object?)entity.BranchId ?? DBNull.Value));
        parameters.Add(new SqlParameter("@DepartmentScope", entity.DepartmentScope));
        parameters.Add(new SqlParameter("@DepartmentId", (object?)entity.DepartmentId ?? DBNull.Value));

        return parameters;
    }
}
