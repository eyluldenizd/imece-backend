using System.Data;
using Infrastructure.Database.DataAccess;
using Infrastructure.Entities;
using Infrastructure.Repositories.Queries;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Repositories;

public sealed class BranchRecord
{
    public int BranchId { get; set; }

    public int? CompanyId { get; set; }

    public string? CompanyName { get; set; }

    public string BranchCode { get; set; } = string.Empty;

    public string BranchName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Address { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

public sealed class BranchRepository
{
    private readonly ISqlDataAccess _dataAccess;

    public BranchRepository(ISqlDataAccess dataAccess)
    {
        _dataAccess = dataAccess;
    }

    public Task<List<BranchRecord>> GetAllAsync(CancellationToken cancellationToken = default) =>
        _dataAccess.QueryAsync<BranchRecord>(BranchQueries.GetAll, null, cancellationToken);

    public Task<List<BranchRecord>> GetActiveAsync(CancellationToken cancellationToken = default) =>
        _dataAccess.QueryAsync<BranchRecord>(BranchQueries.GetActive, null, cancellationToken);

    public Task<List<BranchRecord>> GetByCompanyIdAsync(
        int companyId,
        CancellationToken cancellationToken = default)
    {
        SqlParameter[] parameters =
        [
            new SqlParameter("@CompanyId", SqlDbType.Int) { Value = companyId }
        ];

        return _dataAccess.QueryAsync<BranchRecord>(
            BranchQueries.GetByCompanyId,
            parameters,
            cancellationToken);
    }

    public Task<BranchRecord?> GetByIdAsync(
        int branchId,
        CancellationToken cancellationToken = default)
    {
        SqlParameter[] parameters =
        [
            new SqlParameter("@BranchId", SqlDbType.Int) { Value = branchId }
        ];

        return _dataAccess.QueryFirstOrDefaultAsync<BranchRecord>(
            BranchQueries.GetById,
            parameters,
            cancellationToken);
    }

    public Task<Branches?> GetEntityByIdAsync(
        int branchId,
        CancellationToken cancellationToken = default)
    {
        SqlParameter[] parameters =
        [
            new SqlParameter("@BranchId", SqlDbType.Int) { Value = branchId }
        ];

        return _dataAccess.QueryFirstOrDefaultAsync<Branches>(
            BranchQueries.GetEntityById,
            parameters,
            cancellationToken);
    }

    public async Task<bool> ExistsByCodeInCompanyAsync(
        int companyId,
        string branchCode,
        int? excludeBranchId = null,
        CancellationToken cancellationToken = default)
    {
        SqlParameter[] parameters =
        [
            new SqlParameter("@CompanyId", SqlDbType.Int) { Value = companyId },
            new SqlParameter("@BranchCode", SqlDbType.NVarChar, 64) { Value = branchCode },
            new SqlParameter("@ExcludeBranchId", SqlDbType.Int)
            {
                Value = excludeBranchId.HasValue ? excludeBranchId.Value : DBNull.Value
            }
        ];

        var count = await _dataAccess.ExecuteScalarAsync<int>(
            BranchQueries.ExistsByCodeInCompany,
            parameters,
            cancellationToken);

        return count > 0;
    }

    public Task<int> CreateAsync(
        Branches entity,
        CancellationToken cancellationToken = default) =>
        _dataAccess.ExecuteScalarAsync<int>(
            BranchQueries.Create,
            CreateWriteParameters(entity),
            cancellationToken);

    public Task<int> UpdateAsync(
        Branches entity,
        CancellationToken cancellationToken = default)
    {
        var parameters = CreateWriteParameters(entity).ToList();
        parameters.Add(new SqlParameter("@BranchId", SqlDbType.Int) { Value = entity.BranchId });
        return _dataAccess.ExecuteAsync(BranchQueries.Update, parameters, cancellationToken);
    }

    public Task<int> SoftDeleteAsync(
        int branchId,
        CancellationToken cancellationToken = default)
    {
        SqlParameter[] parameters =
        [
            new SqlParameter("@BranchId", SqlDbType.Int) { Value = branchId }
        ];

        return _dataAccess.ExecuteAsync(
            BranchQueries.SoftDelete,
            parameters,
            cancellationToken);
    }

    public Task<int> DeleteAsync(
        int branchId,
        CancellationToken cancellationToken = default)
    {
        SqlParameter[] parameters =
        [
            new SqlParameter("@BranchId", SqlDbType.Int) { Value = branchId }
        ];

        return _dataAccess.ExecuteAsync(
            BranchQueries.Delete,
            parameters,
            cancellationToken);
    }

    private static SqlParameter[] CreateWriteParameters(Branches entity) =>
    [
        new SqlParameter("@CompanyId", SqlDbType.Int)
        {
            Value = entity.CompanyId ?? (object)DBNull.Value
        },
        new SqlParameter("@BranchCode", SqlDbType.NVarChar, 64) { Value = entity.BranchCode },
        new SqlParameter("@BranchName", SqlDbType.NVarChar, 256) { Value = entity.BranchName },
        new SqlParameter("@Description", SqlDbType.NVarChar, 512)
        {
            Value = entity.Description ?? (object)DBNull.Value
        },
        new SqlParameter("@Address", SqlDbType.NVarChar, 512)
        {
            Value = entity.Address ?? (object)DBNull.Value
        },
        new SqlParameter("@Latitude", SqlDbType.Decimal)
        {
            Precision = 9,
            Scale = 6,
            Value = entity.Latitude ?? (object)DBNull.Value
        },
        new SqlParameter("@Longitude", SqlDbType.Decimal)
        {
            Precision = 9,
            Scale = 6,
            Value = entity.Longitude ?? (object)DBNull.Value
        },
        new SqlParameter("@IsActive", SqlDbType.Bit) { Value = entity.IsActive }
    ];
}
