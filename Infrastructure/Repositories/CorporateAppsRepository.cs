using Core.Entities;
using Infrastructure.Database.DataAccess;
using Infrastructure.Repositories.Queries;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Repositories;

public sealed class CorporateAppsRepository
{
    private readonly ISqlDataAccess _dataAccess;

    public CorporateAppsRepository(ISqlDataAccess dataAccess)
    {
        _dataAccess = dataAccess;
    }

    public Task<List<CorporateApps>> GetAllAsync(CancellationToken cancellationToken = default)
        => _dataAccess.QueryAsync<CorporateApps>(CorporateAppsQueries.GetAll, null, cancellationToken);

    public Task<CorporateApps?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var parameters = new List<SqlParameter> { new("@AppId", id) };
        return _dataAccess.QueryFirstOrDefaultAsync<CorporateApps>(
            CorporateAppsQueries.GetById,
            parameters,
            cancellationToken);
    }

    public Task<long> CreateAsync(CorporateApps entity, CancellationToken cancellationToken = default)
        => _dataAccess.ExecuteScalarAsync<long>(
            CorporateAppsQueries.Create,
            GetWriteParameters(entity, includeId: false),
            cancellationToken);

    public Task<int> UpdateAsync(CorporateApps entity, CancellationToken cancellationToken = default)
        => _dataAccess.ExecuteAsync(
            CorporateAppsQueries.Update,
            GetWriteParameters(entity, includeId: true),
            cancellationToken);

    public Task<int> SoftDeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var parameters = new List<SqlParameter> { new("@AppId", id) };
        return _dataAccess.ExecuteAsync(CorporateAppsQueries.SoftDelete, parameters, cancellationToken);
    }

    private static List<SqlParameter> GetWriteParameters(CorporateApps entity, bool includeId)
    {
        var parameters = new List<SqlParameter>();

        if (includeId)
        {
            parameters.Add(new SqlParameter("@AppId", entity.AppId));
        }

        parameters.Add(new SqlParameter("@Title", entity.Title));
        parameters.Add(new SqlParameter("@Description", (object?)entity.Description ?? DBNull.Value));
        parameters.Add(new SqlParameter("@Url", entity.Url));
        parameters.Add(new SqlParameter("@CorporateAppCategoryId", (object?)entity.CorporateAppCategoryId ?? DBNull.Value));
        parameters.Add(new SqlParameter("@Category", (object?)entity.Category ?? DBNull.Value));
        parameters.Add(new SqlParameter("@IconUrl", (object?)entity.IconUrl ?? DBNull.Value));
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
