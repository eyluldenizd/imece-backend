using Core.Entities;
using Infrastructure.Data;
using Infrastructure.Repositories.Queries;

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
        => _dbManager.QueryFirstOrDefaultAsync<Services>(ServicesQueries.GetById, new { ServiceId = id }, cancellationToken);

    public Task<long> CreateAsync(Services entity, CancellationToken cancellationToken = default) 
        => _dbManager.ExecuteScalarAsync<long>(ServicesQueries.Create, entity, cancellationToken);

    public Task<int> UpdateAsync(Services entity, CancellationToken cancellationToken = default) 
        => _dbManager.ExecuteAsync(ServicesQueries.Update, entity, cancellationToken);

    public Task<int> DeleteAsync(long id, CancellationToken cancellationToken = default) 
        => _dbManager.ExecuteAsync(ServicesQueries.Delete, new { ServiceId = id }, cancellationToken);
}
