using Infrastructure.Database.DataAccess;
using Infrastructure.Entities;
using Infrastructure.Repositories.Queries;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Repositories;

public sealed class CommunicationChannelTypeRepository
{
    private readonly ISqlDataAccess _dataAccess;

    public CommunicationChannelTypeRepository(ISqlDataAccess dataAccess)
    {
        _dataAccess = dataAccess;
    }

    public Task<List<CommunicationChannelTypes>> GetAllAsync(CancellationToken cancellationToken = default)
        => _dataAccess.QueryAsync<CommunicationChannelTypes>(
            CommunicationChannelTypeQueries.GetAll,
            null,
            cancellationToken);

    public Task<CommunicationChannelTypes?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var parameters = new List<SqlParameter> { new("@CommunicationChannelTypeId", id) };
        return _dataAccess.QueryFirstOrDefaultAsync<CommunicationChannelTypes>(
            CommunicationChannelTypeQueries.GetById,
            parameters,
            cancellationToken);
    }

    public Task<CommunicationChannelTypes?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var parameters = new List<SqlParameter> { new("@Code", code) };
        return _dataAccess.QueryFirstOrDefaultAsync<CommunicationChannelTypes>(
            CommunicationChannelTypeQueries.GetByCode,
            parameters,
            cancellationToken);
    }

    public Task<int> CreateAsync(CommunicationChannelTypes entity, CancellationToken cancellationToken = default)
        => _dataAccess.ExecuteScalarAsync<int>(
            CommunicationChannelTypeQueries.Create,
            GetWriteParameters(entity, includeId: false),
            cancellationToken);

    public Task<int> UpdateAsync(CommunicationChannelTypes entity, CancellationToken cancellationToken = default)
        => _dataAccess.ExecuteAsync(
            CommunicationChannelTypeQueries.Update,
            GetWriteParameters(entity, includeId: true),
            cancellationToken);

    public Task<int> SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var parameters = new List<SqlParameter> { new("@CommunicationChannelTypeId", id) };
        return _dataAccess.ExecuteAsync(
            CommunicationChannelTypeQueries.SoftDelete,
            parameters,
            cancellationToken);
    }

    private static List<SqlParameter> GetWriteParameters(CommunicationChannelTypes entity, bool includeId)
    {
        var parameters = new List<SqlParameter>();

        if (includeId)
        {
            parameters.Add(new SqlParameter("@CommunicationChannelTypeId", entity.CommunicationChannelTypeId));
        }

        parameters.Add(new SqlParameter("@Name", entity.Name));
        parameters.Add(new SqlParameter("@Code", entity.Code));
        parameters.Add(new SqlParameter("@IconUrl", (object?)entity.IconUrl ?? DBNull.Value));
        parameters.Add(new SqlParameter("@SortOrder", entity.SortOrder));
        parameters.Add(new SqlParameter("@IsActive", entity.IsActive));

        return parameters;
    }
}
