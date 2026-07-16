using Infrastructure.Data;
using Infrastructure.Entities;
using Infrastructure.Queries;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Infrastructure.Repositories;

public sealed class EmergencyNumberRepository
{
    private readonly DbManager _dbManager;

    public EmergencyNumberRepository(DbManager dbManager)
    {
        _dbManager = dbManager;
    }

    public Task<List<EmergencyNumbers>> GetAllAsync(CancellationToken cancellationToken = default)
        => _dbManager.QueryAsync<EmergencyNumbers>(EmergencyNumberQueries.GetAll, null, cancellationToken);

    public Task<EmergencyNumbers?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        SqlParameter[] parameters =
        [
            new SqlParameter("@EmergencyNumberId", SqlDbType.BigInt) { Value = id }
        ];

        return _dbManager.QueryFirstOrDefaultAsync<EmergencyNumbers>(
            EmergencyNumberQueries.GetById,
            parameters,
            cancellationToken);
    }

    public Task<int> CreateAsync(EmergencyNumbers entity, CancellationToken cancellationToken = default)
        => _dbManager.ExecuteAsync(EmergencyNumberQueries.Create, CreateWriteParameters(entity), cancellationToken);

    public Task<int> UpdateAsync(EmergencyNumbers entity, CancellationToken cancellationToken = default)
        => _dbManager.ExecuteAsync(EmergencyNumberQueries.Update, UpdateWriteParameters(entity), cancellationToken);

    public Task<int> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        SqlParameter[] parameters =
        [
            new SqlParameter("@EmergencyNumberId", SqlDbType.BigInt) { Value = id }
        ];

        return _dbManager.ExecuteAsync(EmergencyNumberQueries.Delete, parameters, cancellationToken);
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
