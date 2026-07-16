using Infrastructure.Data;
using Infrastructure.Entities;
using Infrastructure.Queries;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Infrastructure.Repositories;

public sealed class ECardRepository
{
    private readonly DbManager _dbManager;

    public ECardRepository(DbManager dbManager)
    {
        _dbManager = dbManager;
    }

    public Task<List<ECards>> GetAllAsync(CancellationToken cancellationToken = default)
        => _dbManager.QueryAsync<ECards>(ECardQueries.GetAll, null, cancellationToken);

    public Task<ECards?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        SqlParameter[] parameters =
        [
            new SqlParameter("@ECardId", SqlDbType.BigInt) { Value = id }
        ];

        return _dbManager.QueryFirstOrDefaultAsync<ECards>(
            ECardQueries.GetById,
            parameters,
            cancellationToken);
    }

    public Task<int> CreateAsync(ECards entity, CancellationToken cancellationToken = default)
        => _dbManager.ExecuteAsync(ECardQueries.Create, CreateWriteParameters(entity), cancellationToken);

    public Task<int> UpdateAsync(ECards entity, CancellationToken cancellationToken = default)
        => _dbManager.ExecuteAsync(ECardQueries.Update, UpdateWriteParameters(entity), cancellationToken);

    public Task<int> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        SqlParameter[] parameters =
        [
            new SqlParameter("@ECardId", SqlDbType.BigInt) { Value = id }
        ];

        return _dbManager.ExecuteAsync(ECardQueries.Delete, parameters, cancellationToken);
    }

    private static SqlParameter[] CreateWriteParameters(ECards entity)
    {
        return
        [
            new SqlParameter("@Title", entity.Title),
            new SqlParameter("@Description", (object?)entity.Description ?? DBNull.Value),
            new SqlParameter("@CardType", (object?)entity.CardType ?? DBNull.Value),
            new SqlParameter("@ImageUrl", (object?)entity.ImageUrl ?? DBNull.Value),
            new SqlParameter("@RedirectUrl", (object?)entity.RedirectUrl ?? DBNull.Value),
            new SqlParameter("@IsActive", entity.IsActive),
            new SqlParameter("@DisplayOrder", (object?)entity.DisplayOrder ?? DBNull.Value),
        ];
    }

    private static SqlParameter[] UpdateWriteParameters(ECards entity)
    {
        return
        [
            new SqlParameter("@ECardId", SqlDbType.BigInt) { Value = entity.ECardId },
            new SqlParameter("@Title", entity.Title),
            new SqlParameter("@Description", (object?)entity.Description ?? DBNull.Value),
            new SqlParameter("@CardType", (object?)entity.CardType ?? DBNull.Value),
            new SqlParameter("@ImageUrl", (object?)entity.ImageUrl ?? DBNull.Value),
            new SqlParameter("@RedirectUrl", (object?)entity.RedirectUrl ?? DBNull.Value),
            new SqlParameter("@IsActive", entity.IsActive),
            new SqlParameter("@DisplayOrder", (object?)entity.DisplayOrder ?? DBNull.Value),
        ];
    }
}
