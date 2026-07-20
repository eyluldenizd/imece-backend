using Core.Entities;
using Infrastructure.Database.DataAccess;
using Infrastructure.Repositories.Queries;

namespace Infrastructure.Repositories;

public sealed class UpcomingEventsRepository
{
    private readonly ISqlDataAccess _dataAccess;

    public UpcomingEventsRepository(ISqlDataAccess dataAccess)
    {
        _dataAccess = dataAccess;
    }

    public Task<List<UpcomingEvents>> GetAllAsync(CancellationToken cancellationToken = default) 
        => _dataAccess.QueryAsync<UpcomingEvents>(UpcomingEventsQueries.GetAll, null, cancellationToken);
}
