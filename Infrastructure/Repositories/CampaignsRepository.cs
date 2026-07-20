using Core.Entities;
using Infrastructure.Database.DataAccess;
using Infrastructure.Repositories.Queries;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Repositories;

public sealed class CampaignsRepository
{
    private readonly ISqlDataAccess _dataAccess;

    public CampaignsRepository(ISqlDataAccess dataAccess)
    {
        _dataAccess = dataAccess;
    }

    public Task<List<Campaigns>> GetAllAsync(CancellationToken cancellationToken = default) 
        => _dataAccess.QueryAsync<Campaigns>(CampaignsQueries.GetAll, null, cancellationToken);

    public Task<List<Campaigns>> GetActiveAsync(CancellationToken cancellationToken = default) 
        => _dataAccess.QueryAsync<Campaigns>(CampaignsQueries.GetActive, null, cancellationToken);

    public Task<Campaigns?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var parameters = new List<SqlParameter>
        {
            new SqlParameter("@CampaignId", id)
        };
        return _dataAccess.QueryFirstOrDefaultAsync<Campaigns>(CampaignsQueries.GetById, parameters, cancellationToken);
    }

    public Task<long> CreateAsync(Campaigns entity, CancellationToken cancellationToken = default) 
    {
        var parameters = GetParameters(entity, includeId: false);
        return _dataAccess.ExecuteScalarAsync<long>(CampaignsQueries.Create, parameters, cancellationToken);
    }

    public Task<int> UpdateAsync(Campaigns entity, CancellationToken cancellationToken = default) 
    {
        var parameters = GetParameters(entity, includeId: true);
        return _dataAccess.ExecuteAsync(CampaignsQueries.Update, parameters, cancellationToken);
    }

    public Task<int> SoftDeleteAsync(long id, CancellationToken cancellationToken = default) 
    {
        var parameters = new List<SqlParameter>
        {
            new SqlParameter("@CampaignId", id)
        };
        return _dataAccess.ExecuteAsync(CampaignsQueries.SoftDelete, parameters, cancellationToken);
    }

    // Campaigns entity nesnesini SqlParameter listesine �eviren yard�mc� metot
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
        parameters.Add(new SqlParameter("@CompanyScope", entity.CompanyScope));
        parameters.Add(new SqlParameter("@CompanyId", (object?)entity.CompanyId ?? DBNull.Value));
        parameters.Add(new SqlParameter("@BranchScope", entity.BranchScope));
        parameters.Add(new SqlParameter("@BranchId", (object?)entity.BranchId ?? DBNull.Value));
        parameters.Add(new SqlParameter("@DepartmentScope", entity.DepartmentScope));
        parameters.Add(new SqlParameter("@DepartmentId", (object?)entity.DepartmentId ?? DBNull.Value));

        return parameters;
    }
}