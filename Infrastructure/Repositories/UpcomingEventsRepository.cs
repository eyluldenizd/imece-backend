using Core.Entities;
using Infrastructure.Data;
using Infrastructure.Repositories.Queries;

namespace Infrastructure.Repositories;

public sealed class UpcomingEventsRepository
{
    private readonly DbManager _dbManager;

    public UpcomingEventsRepository(DbManager dbManager)
    {
        _dbManager = dbManager;
    }

    public Task<List<UpcomingEvents>> GetAllAsync(CancellationToken cancellationToken = default) 
        => _dbManager.QueryAsync<UpcomingEvents>(UpcomingEventsQueries.GetAll, null, cancellationToken);
}
