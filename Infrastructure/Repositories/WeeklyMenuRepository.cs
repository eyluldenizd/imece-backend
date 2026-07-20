using Infrastructure.Database.DataAccess;
using Infrastructure.Entities;
using Infrastructure.Repositories.Queries;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Repositories;

public sealed class WeeklyMenuRepository
{
    private readonly ISqlDataAccess _dataAccess;

    public WeeklyMenuRepository(ISqlDataAccess dataAccess)
    {
        _dataAccess = dataAccess;
    }

    public Task<List<WeeklyMenus>> GetAllAsync(
        CompanyListFilter filter,
        CancellationToken cancellationToken = default) =>
        _dataAccess.QueryAsync<WeeklyMenus>(
            WeeklyMenuQueries.GetAll,
            CompanyListFilterParameters.Create(filter),
            cancellationToken);

    public Task<WeeklyMenus?> GetByIdAsync(long menuId, CancellationToken cancellationToken = default)
    {
        var parameters = new List<SqlParameter> { new("@MenuId", menuId) };
        return _dataAccess.QueryFirstOrDefaultAsync<WeeklyMenus>(
            WeeklyMenuQueries.GetById,
            parameters,
            cancellationToken);
    }

    public Task<WeeklyMenus?> GetByCompanyAndCodeAsync(
        int companyId,
        string menuCode,
        CancellationToken cancellationToken = default)
    {
        var parameters = new List<SqlParameter>
        {
            new("@CompanyId", companyId),
            new("@MenuCode", menuCode)
        };

        return _dataAccess.QueryFirstOrDefaultAsync<WeeklyMenus>(
            WeeklyMenuQueries.GetByCompanyAndCode,
            parameters,
            cancellationToken);
    }

    public Task<long> CreateAsync(WeeklyMenus entity, CancellationToken cancellationToken = default)
    {
        var parameters = GetWriteParameters(entity);
        return _dataAccess.ExecuteScalarAsync<long>(WeeklyMenuQueries.Create, parameters, cancellationToken);
    }

    public Task<int> UpdateAsync(WeeklyMenus entity, CancellationToken cancellationToken = default)
    {
        var parameters = new List<SqlParameter>
        {
            new("@MenuId", entity.MenuId),
            new("@Title", (object?)entity.Title ?? DBNull.Value)
        };

        return _dataAccess.ExecuteAsync(WeeklyMenuQueries.Update, parameters, cancellationToken);
    }

    public Task<int> PublishAsync(long menuId, CancellationToken cancellationToken = default)
    {
        var parameters = new List<SqlParameter> { new("@MenuId", menuId) };
        return _dataAccess.ExecuteAsync(WeeklyMenuQueries.Publish, parameters, cancellationToken);
    }

    public Task<int> UnpublishAsync(long menuId, CancellationToken cancellationToken = default)
    {
        var parameters = new List<SqlParameter> { new("@MenuId", menuId) };
        return _dataAccess.ExecuteAsync(WeeklyMenuQueries.Unpublish, parameters, cancellationToken);
    }

    public Task<int> SoftDeleteAsync(long menuId, CancellationToken cancellationToken = default)
    {
        var parameters = new List<SqlParameter> { new("@MenuId", menuId) };
        return _dataAccess.ExecuteAsync(WeeklyMenuQueries.SoftDelete, parameters, cancellationToken);
    }

    private static List<SqlParameter> GetWriteParameters(WeeklyMenus entity)
    {
        return
        [
            new SqlParameter("@CompanyId", entity.CompanyId),
            new SqlParameter("@MenuCode", entity.MenuCode),
            new SqlParameter("@Year", entity.Year),
            new SqlParameter("@Month", entity.Month),
            new SqlParameter("@WeekOfMonth", entity.WeekOfMonth),
            new SqlParameter("@PeriodStartDate", entity.PeriodStartDate.ToDateTime(TimeOnly.MinValue)),
            new SqlParameter("@PeriodEndDate", entity.PeriodEndDate.ToDateTime(TimeOnly.MinValue)),
            new SqlParameter("@Title", (object?)entity.Title ?? DBNull.Value),
            new SqlParameter("@CreatedBy", entity.CreatedBy)
        ];
    }
}
