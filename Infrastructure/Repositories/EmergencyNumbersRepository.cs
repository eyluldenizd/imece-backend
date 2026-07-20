using Infrastructure.Database.DataAccess;
using Infrastructure.Entities;
using Infrastructure.Queries;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Infrastructure.Repositories;

public sealed class EmergencyNumberRepository
{
    private readonly ISqlDataAccess _dataAccess;

    public EmergencyNumberRepository(ISqlDataAccess dataAccess)
    {
        _dataAccess = dataAccess;
    }

    public Task<List<EmergencyNumbers>> GetAllAsync(CancellationToken cancellationToken = default)
        => _dataAccess.QueryAsync<EmergencyNumbers>(EmergencyNumberQueries.GetAll, null, cancellationToken);

    public Task<EmergencyNumbers?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        SqlParameter[] parameters =
        [
            new SqlParameter("@EmergencyNumberId", SqlDbType.BigInt) { Value = id }
        ];

        return _dataAccess.QueryFirstOrDefaultAsync<EmergencyNumbers>(
            EmergencyNumberQueries.GetById,
            parameters,
            cancellationToken);
    }

    public Task<int> CreateAsync(EmergencyNumbers entity, CancellationToken cancellationToken = default)
        => _dataAccess.ExecuteAsync(EmergencyNumberQueries.Create, CreateWriteParameters(entity), cancellationToken);

    public Task<int> UpdateAsync(EmergencyNumbers entity, CancellationToken cancellationToken = default)
        => _dataAccess.ExecuteAsync(EmergencyNumberQueries.Update, UpdateWriteParameters(entity), cancellationToken);

    public Task<int> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        SqlParameter[] parameters =
        [
            new SqlParameter("@EmergencyNumberId", SqlDbType.BigInt) { Value = id }
        ];

        return _dataAccess.ExecuteAsync(EmergencyNumberQueries.Delete, parameters, cancellationToken);
    }

    private static SqlParameter[] CreateWriteParameters(EmergencyNumbers entity)
    {
        return
        [
            new SqlParameter("@Name", entity.Name),
            new SqlParameter("@PhoneNumber", entity.PhoneNumber),
            new SqlParameter("@Category", entity.Category),
            new SqlParameter("@Description", (object?)entity.Description ?? DBNull.Value),
            new SqlParameter("@IsActive", entity.IsActive),
            new SqlParameter("@DisplayOrder", (object?)entity.DisplayOrder ?? DBNull.Value),
        ];
    }

    private static SqlParameter[] UpdateWriteParameters(EmergencyNumbers entity)
    {
        return
        [
            new SqlParameter("@EmergencyNumberId", SqlDbType.BigInt) { Value = entity.EmergencyNumberId },
            new SqlParameter("@Name", entity.Name),
            new SqlParameter("@PhoneNumber", entity.PhoneNumber),
            new SqlParameter("@Category", entity.Category),
            new SqlParameter("@Description", (object?)entity.Description ?? DBNull.Value),
            new SqlParameter("@IsActive", entity.IsActive),
            new SqlParameter("@DisplayOrder", (object?)entity.DisplayOrder ?? DBNull.Value),
        ];
    }
}
