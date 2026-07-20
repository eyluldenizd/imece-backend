using Infrastructure.Database.DataAccess;
using Infrastructure.Entities;
using Infrastructure.Repositories.Queries;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Repositories;

public sealed class WeeklyMenuItemRepository
{
    private readonly ISqlDataAccess _dataAccess;

    public WeeklyMenuItemRepository(ISqlDataAccess dataAccess)
    {
        _dataAccess = dataAccess;
    }

    public Task<List<WeeklyMenuItems>> GetByMenuIdAsync(
        long menuId,
        CancellationToken cancellationToken = default)
    {
        var parameters = new List<SqlParameter> { new("@MenuId", menuId) };
        return _dataAccess.QueryAsync<WeeklyMenuItems>(
            WeeklyMenuItemQueries.GetByMenuId,
            parameters,
            cancellationToken);
    }

    public Task<WeeklyMenuItems?> GetByIdAsync(
        long menuItemId,
        CancellationToken cancellationToken = default)
    {
        var parameters = new List<SqlParameter> { new("@MenuItemId", menuItemId) };
        return _dataAccess.QueryFirstOrDefaultAsync<WeeklyMenuItems>(
            WeeklyMenuItemQueries.GetById,
            parameters,
            cancellationToken);
    }

    public Task<WeeklyMenuItems?> GetByIdAndMenuIdAsync(
        long menuId,
        long menuItemId,
        CancellationToken cancellationToken = default)
    {
        var parameters = new List<SqlParameter>
        {
            new("@MenuId", menuId),
            new("@MenuItemId", menuItemId)
        };

        return _dataAccess.QueryFirstOrDefaultAsync<WeeklyMenuItems>(
            WeeklyMenuItemQueries.GetByIdAndMenuId,
            parameters,
            cancellationToken);
    }

    public async Task<bool> ExistsDuplicateAsync(
        long menuId,
        DateOnly menuDate,
        int dishCategoryId,
        int dishId,
        long? excludeMenuItemId = null,
        CancellationToken cancellationToken = default)
    {
        var parameters = new List<SqlParameter>
        {
            new("@MenuId", menuId),
            new("@MenuDate", menuDate.ToDateTime(TimeOnly.MinValue)),
            new("@DishCategoryId", dishCategoryId),
            new("@DishId", dishId),
            new("@ExcludeMenuItemId", (object?)excludeMenuItemId ?? DBNull.Value)
        };

        var result = await _dataAccess.ExecuteScalarAsync<int?>(
            WeeklyMenuItemQueries.ExistsDuplicate,
            parameters,
            cancellationToken);

        return result.HasValue;
    }

    public Task<long> CreateAsync(WeeklyMenuItems entity, CancellationToken cancellationToken = default)
    {
        var parameters = GetWriteParameters(entity, includeIds: false);
        return _dataAccess.ExecuteScalarAsync<long>(WeeklyMenuItemQueries.Create, parameters, cancellationToken);
    }

    public Task<int> UpdateAsync(WeeklyMenuItems entity, CancellationToken cancellationToken = default)
    {
        var parameters = GetWriteParameters(entity, includeIds: true);
        return _dataAccess.ExecuteAsync(WeeklyMenuItemQueries.Update, parameters, cancellationToken);
    }

    public Task<int> SoftDeleteAsync(
        long menuId,
        long menuItemId,
        CancellationToken cancellationToken = default)
    {
        var parameters = new List<SqlParameter>
        {
            new("@MenuId", menuId),
            new("@MenuItemId", menuItemId)
        };

        return _dataAccess.ExecuteAsync(WeeklyMenuItemQueries.SoftDelete, parameters, cancellationToken);
    }

    private static List<SqlParameter> GetWriteParameters(WeeklyMenuItems entity, bool includeIds)
    {
        var parameters = new List<SqlParameter>
        {
            new("@MenuId", entity.MenuId),
            new("@MenuDate", entity.MenuDate.ToDateTime(TimeOnly.MinValue)),
            new("@DishCategoryId", entity.DishCategoryId),
            new("@DishId", entity.DishId),
            new("@SortOrder", entity.SortOrder),
            new("@Notes", (object?)entity.Notes ?? DBNull.Value)
        };

        if (includeIds)
        {
            parameters.Add(new SqlParameter("@MenuItemId", entity.MenuItemId));
        }

        return parameters;
    }
}
