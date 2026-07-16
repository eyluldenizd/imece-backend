using Core.Entities;
using Infrastructure.Data;
using Infrastructure.Repositories.Queries;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Repositories;

public sealed class ServicesRepository
{
    private readonly DbManager _dbManager;

    public ServicesRepository(DbManager dbManager)
    {
        _dbManager = dbManager;
    }

    public Task<List<Services>> GetAllAsync(CancellationToken cancellationToken = default) 
        => _dbManager.QueryAsync<Services>(ServicesQueries.GetAll, null, cancellationToken);

    public Task<List<Services>> GetActiveAsync(CancellationToken cancellationToken = default) 
        => _dbManager.QueryAsync<Services>(ServicesQueries.GetActive, null, cancellationToken);

    public Task<Services?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var parameters = new List<SqlParameter>
        {
            new("@ServiceId", id)
        };
        return _dbManager.QueryFirstOrDefaultAsync<Services>(ServicesQueries.GetById, parameters, cancellationToken);
    }

    public Task<long> CreateAsync(Services entity, CancellationToken cancellationToken = default)
    {
        var parameters = GetWriteParameters(entity, includeId: false);
        return _dbManager.ExecuteScalarAsync<long>(ServicesQueries.Create, parameters, cancellationToken);
    }

    public Task<int> UpdateAsync(Services entity, CancellationToken cancellationToken = default)
    {
        var parameters = GetWriteParameters(entity, includeId: true);
        return _dbManager.ExecuteAsync(ServicesQueries.Update, parameters, cancellationToken);
    }

    public Task<int> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var parameters = new List<SqlParameter>
        {
            new("@ServiceId", id)
        };
        return _dbManager.ExecuteAsync(ServicesQueries.Delete, parameters, cancellationToken);
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

        return parameters;
    }
}
