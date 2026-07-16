using Core.Entities;
using Infrastructure.Data;
using Infrastructure.Repositories.Queries;

namespace Infrastructure.Repositories;

public sealed class CommunicationChannelsRepository
{
    private readonly DbManager _dbManager;

    public CommunicationChannelsRepository(DbManager dbManager)
    {
        _dbManager = dbManager;
    }

    public Task<List<CommunicationChannels>> GetAllAsync(CancellationToken cancellationToken = default) 
        => _dbManager.QueryAsync<CommunicationChannels>(CommunicationChannelsQueries.GetAll, null, cancellationToken);
}
