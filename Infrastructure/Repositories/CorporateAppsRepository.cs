using Core.Entities;
using Infrastructure.Database.DataAccess;
using Infrastructure.Repositories.Queries;

namespace Infrastructure.Repositories;

public sealed class CorporateAppsRepository
{
    private readonly ISqlDataAccess _dataAccess;

    public CorporateAppsRepository(ISqlDataAccess dataAccess)
    {
        _dataAccess = dataAccess;
    }

    public Task<List<CorporateApps>> GetAllAsync(CancellationToken cancellationToken = default) 
        => _dataAccess.QueryAsync<CorporateApps>(CorporateAppsQueries.GetAll, null, cancellationToken);
}
