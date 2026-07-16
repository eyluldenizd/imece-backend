using Core.Entities;
using Infrastructure.Data;
using Infrastructure.Repositories.Queries;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Repositories;

public sealed class CampaignsRepository
{
    private readonly DbManager _dbManager;

    public CampaignsRepository(DbManager dbManager)
    {
        _dbManager = dbManager;
    }

    public Task<List<Campaigns>> GetAllAsync(CancellationToken cancellationToken = default) 
        => _dbManager.QueryAsync<Campaigns>(CampaignsQueries.GetAll, null, cancellationToken);

    public Task<List<Campaigns>> GetActiveAsync(CancellationToken cancellationToken = default) 
        => _dbManager.QueryAsync<Campaigns>(CampaignsQueries.GetActive, null, cancellationToken);

    public Task<Campaigns?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var parameters = new List<SqlParameter>
        {
            new SqlParameter("@CampaignId", id)
        };
        return _dbManager.QueryFirstOrDefaultAsync<Campaigns>(CampaignsQueries.GetById, parameters, cancellationToken);
    }

    public Task<long> CreateAsync(Campaigns entity, CancellationToken cancellationToken = default) 
    {
        var parameters = GetParameters(entity, includeId: false);
        return _dbManager.ExecuteScalarAsync<long>(CampaignsQueries.Create, parameters, cancellationToken);
    }

    public Task<int> UpdateAsync(Campaigns entity, CancellationToken cancellationToken = default) 
    {
        var parameters = GetParameters(entity, includeId: true);
        return _dbManager.ExecuteAsync(CampaignsQueries.Update, parameters, cancellationToken);
    }

    public Task<int> DeleteAsync(long id, CancellationToken cancellationToken = default) 
    {
        var parameters = new List<SqlParameter>
        {
            new SqlParameter("@CampaignId", id)
        };
        return _dbManager.ExecuteAsync(CampaignsQueries.Delete, parameters, cancellationToken);
    }

    // Campaigns entity nesnesini SqlParameter listesine çeviren yardımcı metot
    private static List<SqlParameter> GetParameters(Campaigns entity, bool includeId)
    {
        var parameters = new List<SqlParameter>();

        if (includeId)
        {
            parameters.Add(new SqlParameter("@CampaignId", entity.CampaignId));
        }

        parameters.Add(new SqlParameter("@Title", entity.Title));
        parameters.Add(new SqlParameter("@Description", (object?)entity.Description ?? DBNull.Value));
        parameters.Add(new SqlParameter("@ImageUrl", (object?)entity.ImageUrl ?? DBNull.Value));
        parameters.Add(new SqlParameter("@TargetUrl", (object?)entity.TargetUrl ?? DBNull.Value));
        parameters.Add(new SqlParameter("@StartDate", entity.StartDate));
        parameters.Add(new SqlParameter("@EndDate", entity.EndDate));
        parameters.Add(new SqlParameter("@IsActive", entity.IsActive));

        return parameters;
    }
}