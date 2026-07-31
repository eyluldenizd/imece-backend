using System.Data;
using Infrastructure.Database.DataAccess;
using Infrastructure.Data;
using Infrastructure.Entities;
using Infrastructure.Repositories.Queries;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Repositories;

public sealed class WeeklyMenuEntryDetails
{
    [DbManager.DbColumn("entry_id")]
    public long EntryId { get; set; }

    [DbManager.DbColumn("week_id")]
    public int WeekId { get; set; }

    [DbManager.DbColumn("week_year")]
    public short Year { get; set; }

    [DbManager.DbColumn("week_number")]
    public byte WeekNumber { get; set; }

    [DbManager.DbColumn("week_start_date")]
    public DateOnly WeekStartDate { get; set; }

    [DbManager.DbColumn("week_end_date")]
    public DateOnly WeekEndDate { get; set; }

    [DbManager.DbColumn("dish_id")]
    public int DishId { get; set; }

    [DbManager.DbColumn("dish_name")]
    public string DishName { get; set; } = string.Empty;

    [DbManager.DbColumn("dish_category")]
    public string DishCategory { get; set; } = string.Empty;

    [DbManager.DbColumn("branch_id")]
    public int BranchId { get; set; }

    [DbManager.DbColumn("branch_name")]
    public string BranchName { get; set; } = string.Empty;

    [DbManager.DbColumn("company_id")]
    public int CompanyId { get; set; }

    [DbManager.DbColumn("menu_date")]
    public DateOnly MenuDate { get; set; }

    [DbManager.DbColumn("meal_type")]
    public string MealType { get; set; } = string.Empty;

    [DbManager.DbColumn("sort_order")]
    public short SortOrder { get; set; }

    [DbManager.DbColumn("created_by")]
    public int? CreatedBy { get; set; }

    [DbManager.DbColumn("created_by_full_name")]
    public string? CreatedByFullName { get; set; }

    [DbManager.DbColumn("created_at")]
    public DateTime CreatedAt { get; set; }
}

public sealed class WeeklyMenuEntryRepository
{
    private readonly ISqlDataAccess _dataAccess;

    public WeeklyMenuEntryRepository(
        ISqlDataAccess dataAccess)
    {
        _dataAccess = dataAccess;
    }

    public Task<List<WeeklyMenuEntryDetails>> GetAllAsync(
        CompanyListFilter filter,
        CancellationToken cancellationToken = default)
    {
        return _dataAccess.QueryAsync<WeeklyMenuEntryDetails>(
            WeeklyMenuEntryQueries.GetAll,
            CompanyListFilterParameters.Create(filter),
            cancellationToken);
    }

    public Task<WeeklyMenuEntryDetails?> GetByIdAsync(
        long entryId,
        CancellationToken cancellationToken = default)
    {
        SqlParameter[] parameters =
        [
            new SqlParameter(
                "@EntryId",
                SqlDbType.BigInt)
            {
                Value = entryId
            }
        ];

        return _dataAccess
            .QueryFirstOrDefaultAsync<WeeklyMenuEntryDetails>(
                WeeklyMenuEntryQueries.GetById,
                parameters,
                cancellationToken);
    }

    public Task<List<WeeklyMenuEntryDetails>>
        GetCurrentWeekAsync(
            CompanyListFilter filter,
            CancellationToken cancellationToken = default)
    {
        return _dataAccess.QueryAsync<WeeklyMenuEntryDetails>(
            WeeklyMenuEntryQueries.GetCurrentWeek,
            CompanyListFilterParameters.Create(filter),
            cancellationToken);
    }

    public Task<List<WeeklyMenuEntryDetails>> GetByDateAsync(
        DateOnly menuDate,
        CompanyListFilter filter,
        CancellationToken cancellationToken = default)
    {
        return _dataAccess.QueryAsync<WeeklyMenuEntryDetails>(
            WeeklyMenuEntryQueries.GetByDate,
            CompanyListFilterParameters.Combine(
                filter,
                new SqlParameter(
                    "@MenuDate",
                    SqlDbType.Date)
                {
                    Value = menuDate.ToDateTime(
                        TimeOnly.MinValue)
                }),
            cancellationToken);
    }

    public Task<List<WeeklyMenuEntryDetails>> GetByBranchAsync(
        int branchId,
        CompanyListFilter filter,
        CancellationToken cancellationToken = default)
    {
        return _dataAccess.QueryAsync<WeeklyMenuEntryDetails>(
            WeeklyMenuEntryQueries.GetByBranch,
            CompanyListFilterParameters.Combine(
                filter,
                new SqlParameter(
                    "@BranchId",
                    SqlDbType.Int)
                {
                    Value = branchId
                }),
            cancellationToken);
    }

    public async Task<bool> IsMenuDateInWeekAsync(
        int weekId,
        DateOnly menuDate,
        CancellationToken cancellationToken = default)
    {
        SqlParameter[] parameters =
        [
            new SqlParameter(
                "@WeekId",
                SqlDbType.Int)
            {
                Value = weekId
            },

            new SqlParameter(
                "@MenuDate",
                SqlDbType.Date)
            {
                Value = menuDate.ToDateTime(
                    TimeOnly.MinValue)
            }
        ];

        var count = await _dataAccess.ExecuteScalarAsync<int>(
            WeeklyMenuEntryQueries.IsMenuDateInWeek,
            parameters,
            cancellationToken);

        return count > 0;
    }

    public Task<long> CreateAsync(
        WeeklyMenuEntries entity,
        CancellationToken cancellationToken = default)
    {
        var parameters = CreateWriteParameters(entity);

        return _dataAccess.ExecuteScalarAsync<long>(
            WeeklyMenuEntryQueries.Create,
            parameters,
            cancellationToken);
    }

    public Task<int> UpdateAsync(
        WeeklyMenuEntries entity,
        CancellationToken cancellationToken = default)
    {
        var parameters = CreateWriteParameters(entity)
            .ToList();

        parameters.Add(
            new SqlParameter(
                "@EntryId",
                SqlDbType.BigInt)
            {
                Value = entity.EntryId
            });

        return _dataAccess.ExecuteAsync(
            WeeklyMenuEntryQueries.Update,
            parameters,
            cancellationToken);
    }

    public Task<int> DeleteAsync(
        long entryId,
        CancellationToken cancellationToken = default)
    {
        SqlParameter[] parameters =
        [
            new SqlParameter(
                "@EntryId",
                SqlDbType.BigInt)
            {
                Value = entryId
            }
        ];

        return _dataAccess.ExecuteAsync(
            WeeklyMenuEntryQueries.Delete,
            parameters,
            cancellationToken);
    }

    private static SqlParameter[] CreateWriteParameters(
        WeeklyMenuEntries entity)
    {
        return
        [
            new SqlParameter(
                "@WeekId",
                SqlDbType.Int)
            {
                Value = entity.WeekId
            },

            new SqlParameter(
                "@DishId",
                SqlDbType.Int)
            {
                Value = entity.DishId
            },

            new SqlParameter(
                "@BranchId",
                SqlDbType.Int)
            {
                Value = entity.BranchId
            },

            new SqlParameter(
                "@MenuDate",
                SqlDbType.Date)
            {
                Value = entity.MenuDate.ToDateTime(
                    TimeOnly.MinValue)
            },

            new SqlParameter(
                "@MealType",
                SqlDbType.NVarChar,
                20)
            {
                Value = entity.MealType
            },

            new SqlParameter(
                "@SortOrder",
                SqlDbType.SmallInt)
            {
                Value = entity.SortOrder
            },

            new SqlParameter(
                "@CreatedBy",
                SqlDbType.Int)
            {
                Value = entity.CreatedBy
                    ?? (object)DBNull.Value
            }
        ];
    }
}