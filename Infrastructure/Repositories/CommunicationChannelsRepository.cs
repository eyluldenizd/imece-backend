using Core.Entities;
using Infrastructure.Database.DataAccess;
using Infrastructure.Repositories.Queries;

namespace Infrastructure.Repositories;

public sealed class CommunicationChannelsRepository
{
    private readonly ISqlDataAccess _dataAccess;

    public CommunicationChannelsRepository(ISqlDataAccess dataAccess)
    {
        _dataAccess = dataAccess;
    }

    public Task<List<CommunicationChannels>> GetAllAsync(CancellationToken cancellationToken = default) 
        => _dataAccess.QueryAsync<CommunicationChannels>(CommunicationChannelsQueries.GetAll, null, cancellationToken);
}
