using System.Data;
using Infrastructure.Database.DataAccess;
using Infrastructure.Repositories.Queries;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Repositories;

public sealed class DepartmentRecord
{
    public int DepartmentId { get; set; }

    public int? BranchId { get; set; }

    public int? CompanyId { get; set; }

    public int? ParentDepartmentId { get; set; }

    public string? DepartmentCode { get; set; }

    public string DepartmentName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

public sealed class DepartmentRepository
{
    private readonly ISqlDataAccess _dataAccess;

    public DepartmentRepository(ISqlDataAccess dataAccess)
    {
        _dataAccess = dataAccess;
    }

    public Task<List<DepartmentRecord>> GetAllAsync(CancellationToken cancellationToken = default) =>
        _dataAccess.QueryAsync<DepartmentRecord>(DepartmentQueries.GetAll, null, cancellationToken);

    public Task<List<DepartmentRecord>> GetActiveAsync(CancellationToken cancellationToken = default) =>
        _dataAccess.QueryAsync<DepartmentRecord>(DepartmentQueries.GetActive, null, cancellationToken);

    public Task<List<DepartmentRecord>> GetByBranchIdAsync(
        int branchId,
        CancellationToken cancellationToken = default)
    {
        SqlParameter[] parameters =
        [
            new SqlParameter("@BranchId", SqlDbType.Int) { Value = branchId }
        ];

        return _dataAccess.QueryAsync<DepartmentRecord>(
            DepartmentQueries.GetByBranchId,
            parameters,
            cancellationToken);
    }

    public Task<List<DepartmentRecord>> GetByCompanyIdAsync(
        int companyId,
        CancellationToken cancellationToken = default)
    {
        SqlParameter[] parameters =
        [
            new SqlParameter("@CompanyId", SqlDbType.Int) { Value = companyId }
        ];

        return _dataAccess.QueryAsync<DepartmentRecord>(
            DepartmentQueries.GetByCompanyId,
            parameters,
            cancellationToken);
    }

    public Task<DepartmentRecord?> GetByIdAsync(
        int departmentId,
        CancellationToken cancellationToken = default)
    {
        SqlParameter[] parameters =
        [
            new SqlParameter("@DepartmentId", SqlDbType.Int) { Value = departmentId }
        ];

        return _dataAccess.QueryFirstOrDefaultAsync<DepartmentRecord>(
            DepartmentQueries.GetById,
            parameters,
            cancellationToken);
    }

    public async Task<bool> ExistsByCodeInBranchAsync(
        int branchId,
        string departmentCode,
        int? excludeDepartmentId = null,
        CancellationToken cancellationToken = default)
    {
        SqlParameter[] parameters =
        [
            new SqlParameter("@BranchId", SqlDbType.Int) { Value = branchId },
            new SqlParameter("@DepartmentCode", SqlDbType.NVarChar, 64) { Value = departmentCode },
            new SqlParameter("@ExcludeDepartmentId", SqlDbType.Int)
            {
                Value = excludeDepartmentId.HasValue ? excludeDepartmentId.Value : DBNull.Value
            }
        ];

        var count = await _dataAccess.ExecuteScalarAsync<int>(
            DepartmentQueries.ExistsByCodeInBranch,
            parameters,
            cancellationToken);

        return count > 0;
    }

    public Task<int> CreateAsync(
        DepartmentRecord entity,
        CancellationToken cancellationToken = default) =>
        _dataAccess.ExecuteScalarAsync<int>(
            DepartmentQueries.Create,
            CreateWriteParameters(entity),
            cancellationToken);

    public Task<int> UpdateAsync(
        DepartmentRecord entity,
        CancellationToken cancellationToken = default)
    {
        var parameters = CreateWriteParameters(entity).ToList();
        parameters.Add(new SqlParameter("@DepartmentId", SqlDbType.Int) { Value = entity.DepartmentId });
        return _dataAccess.ExecuteAsync(DepartmentQueries.Update, parameters, cancellationToken);
    }

    public Task<int> SoftDeleteAsync(
        int departmentId,
        CancellationToken cancellationToken = default)
    {
        SqlParameter[] parameters =
        [
            new SqlParameter("@DepartmentId", SqlDbType.Int) { Value = departmentId }
        ];

        return _dataAccess.ExecuteAsync(
            DepartmentQueries.SoftDelete,
            parameters,
            cancellationToken);
    }

    private static SqlParameter[] CreateWriteParameters(DepartmentRecord entity) =>
    [
        new SqlParameter("@BranchId", SqlDbType.Int)
        {
            Value = entity.BranchId ?? (object)DBNull.Value
        },
        new SqlParameter("@ParentDepartmentId", SqlDbType.Int)
        {
            Value = entity.ParentDepartmentId ?? (object)DBNull.Value
        },
        new SqlParameter("@DepartmentCode", SqlDbType.NVarChar, 64)
        {
            Value = entity.DepartmentCode ?? (object)DBNull.Value
        },
        new SqlParameter("@DepartmentName", SqlDbType.NVarChar, 256) { Value = entity.DepartmentName },
        new SqlParameter("@Description", SqlDbType.NVarChar, 512)
        {
            Value = entity.Description ?? (object)DBNull.Value
        },
        new SqlParameter("@IsActive", SqlDbType.Bit) { Value = entity.IsActive }
    ];
}
