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
        => _dataAccess.QueryAsync<CommunicationChannels>(
            CommunicationChannelsQueries.GetAll,
            null,
            cancellationToken);

    public Task<CommunicationChannels?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var parameters = new List<SqlParameter>
        {
            new("@ChannelId", id)
        };

        return _dataAccess.QueryFirstOrDefaultAsync<CommunicationChannels>(
            CommunicationChannelsQueries.GetById,
            parameters,
            cancellationToken);
    }

    public Task<long> CreateAsync(CommunicationChannels entity, CancellationToken cancellationToken = default)
        => _dataAccess.ExecuteScalarAsync<long>(
            CommunicationChannelsQueries.Create,
            GetWriteParameters(entity, includeId: false),
            cancellationToken);

    public Task<int> UpdateAsync(CommunicationChannels entity, CancellationToken cancellationToken = default)
        => _dataAccess.ExecuteAsync(
            CommunicationChannelsQueries.Update,
            GetWriteParameters(entity, includeId: true),
            cancellationToken);

    public Task<int> SoftDeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var parameters = new List<SqlParameter>
        {
            new("@ChannelId", id)
        };

        return _dataAccess.ExecuteAsync(
            CommunicationChannelsQueries.SoftDelete,
            parameters,
            cancellationToken);
    }

    private static List<SqlParameter> GetWriteParameters(CommunicationChannels entity, bool includeId)
    {
        var parameters = new List<SqlParameter>();

        if (includeId)
        {
            parameters.Add(new SqlParameter("@ChannelId", entity.ChannelId));
        }

        parameters.Add(new SqlParameter("@ChannelName", entity.ChannelName));
        parameters.Add(new SqlParameter("@Type", entity.Type));
        parameters.Add(new SqlParameter("@CommunicationChannelTypeId", (object?)entity.CommunicationChannelTypeId ?? DBNull.Value));
        parameters.Add(new SqlParameter("@AddressUrl", entity.AddressUrl));
        parameters.Add(new SqlParameter("@DepartmentInCharge", (object?)entity.DepartmentInCharge ?? DBNull.Value));
        parameters.Add(new SqlParameter("@Description", (object?)entity.Description ?? DBNull.Value));
        parameters.Add(new SqlParameter("@Icon", (object?)entity.Icon ?? DBNull.Value));
        parameters.Add(new SqlParameter("@SortOrder", entity.SortOrder));
        parameters.Add(new SqlParameter("@IsActive", entity.IsActive));
        parameters.Add(new SqlParameter("@CompanyScope", entity.CompanyScope));
        parameters.Add(new SqlParameter("@CompanyId", (object?)entity.CompanyId ?? DBNull.Value));
        parameters.Add(new SqlParameter("@BranchScope", entity.BranchScope));
        parameters.Add(new SqlParameter("@BranchId", (object?)entity.BranchId ?? DBNull.Value));
        parameters.Add(new SqlParameter("@DepartmentScope", entity.DepartmentScope));
        parameters.Add(new SqlParameter("@DepartmentId", (object?)entity.DepartmentId ?? DBNull.Value));

        return parameters;
    }
}
