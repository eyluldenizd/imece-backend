using Core.Entities;
using Infrastructure.Data;
using Infrastructure.Repositories.Queries;

namespace Infrastructure.Repositories;

public sealed class CorporateAppsRepository
{
    private readonly DbManager _dbManager;

    public CorporateAppsRepository(DbManager dbManager)
    {
        _dbManager = dbManager;
    }

    public Task<List<CorporateApps>> GetAllAsync(CancellationToken cancellationToken = default) 
        => _dbManager.QueryAsync<CorporateApps>(CorporateAppsQueries.GetAll, null, cancellationToken);
}
