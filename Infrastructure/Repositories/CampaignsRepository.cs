using Core.Entities;
using Infrastructure.Data;
using Infrastructure.Repositories.Queries;

namespace Infrastructure.Repositories;

public sealed class CampaignsRepository
{
    private readonly DbManager _dbManager;

    public CampaignsRepository(DbManager dbManager)
    {
        _dbManager = dbManager;
    }

    public Task<List<Campaigns>> GetAllAsync(CancellationToken cancellationToken = default) 
        => _dbManager.QueryAsync<Campaigns>(CampaignsQueries.GetAll, null, cancellationToken);

    public Task<List<Campaigns>> GetActiveAsync(CancellationToken cancellationToken = default) 
        => _dbManager.QueryAsync<Campaigns>(CampaignsQueries.GetActive, null, cancellationToken);

    public Task<Campaigns?> GetByIdAsync(long id, CancellationToken cancellationToken = default) 
        => _dbManager.QueryFirstOrDefaultAsync<Campaigns>(CampaignsQueries.GetById, new { CampaignId = id }, cancellationToken);

    public Task<long> CreateAsync(Campaigns entity, CancellationToken cancellationToken = default) 
        => _dbManager.ExecuteScalarAsync<long>(CampaignsQueries.Create, entity, cancellationToken);

    public Task<int> UpdateAsync(Campaigns entity, CancellationToken cancellationToken = default) 
        => _dbManager.ExecuteAsync(CampaignsQueries.Update, entity, cancellationToken);

    public Task<int> DeleteAsync(long id, CancellationToken cancellationToken = default) 
        => _dbManager.ExecuteAsync(CampaignsQueries.Delete, new { CampaignId = id }, cancellationToken);
}
