using Core.Entities;
using Infrastructure.Database.DataAccess;
using Infrastructure.Repositories.Queries;
using Microsoft.Data.SqlClient;

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

    public Task<CommunicationChannels?> GetByIdAsync(long id, CancellationToken cancellationToken = default) =>
        _dataAccess.QueryFirstOrDefaultAsync<CommunicationChannels>(
            "SELECT channel_id, channel_name, type, address_url, department_in_charge FROM communication_channels WHERE channel_id=@Id",
            [new("@Id", id)], cancellationToken);

    public Task<long> CreateAsync(CommunicationChannels entity, CancellationToken cancellationToken = default) =>
        _dataAccess.ExecuteScalarAsync<long>(
            "INSERT INTO communication_channels(channel_name,type,address_url,department_in_charge) OUTPUT INSERTED.channel_id VALUES(@Name,@Type,@Url,@Department)",
            Parameters(entity, false), cancellationToken)!;

    public Task<int> UpdateAsync(CommunicationChannels entity, CancellationToken cancellationToken = default) =>
        _dataAccess.ExecuteAsync("UPDATE communication_channels SET channel_name=@Name,type=@Type,address_url=@Url,department_in_charge=@Department WHERE channel_id=@Id",
            Parameters(entity, true), cancellationToken);

    public Task<int> DeleteAsync(long id, CancellationToken cancellationToken = default) =>
        _dataAccess.ExecuteAsync("DELETE FROM communication_channels WHERE channel_id=@Id", [new("@Id", id)], cancellationToken);

    private static SqlParameter[] Parameters(CommunicationChannels x, bool includeId)
    {
        var values = new List<SqlParameter> { new("@Name",x.ChannelName),new("@Type",x.Type),new("@Url",x.AddressUrl),new("@Department",(object?)x.DepartmentInCharge??DBNull.Value) };
        if(includeId) values.Add(new("@Id",x.ChannelId));
        return values.ToArray();
    }
}
