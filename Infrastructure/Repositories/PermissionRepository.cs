using System.Data;
using Infrastructure.Database.DataAccess;
using Infrastructure.Repositories.Queries;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Repositories;

public sealed class PermissionRecord
{
    public int PermissionId { get; set; }

    public string PermissionCode { get; set; } = string.Empty;

    public string? Description { get; set; }
}

public sealed class PermissionRoleCountRow
{
    public int PermissionId { get; set; }

    public int RoleCount { get; set; }
}

public sealed class PermissionRepository
{
    private readonly ISqlDataAccess _dataAccess;

    public PermissionRepository(ISqlDataAccess dataAccess)
    {
        _dataAccess = dataAccess;
    }

    public Task<List<PermissionRecord>> GetAllAsync(CancellationToken cancellationToken = default) =>
        _dataAccess.QueryAsync<PermissionRecord>(PermissionQueries.GetAll, null, cancellationToken);

    public async Task<PermissionRecord?> GetByIdAsync(
        int permissionId,
        CancellationToken cancellationToken = default)
    {
        SqlParameter[] parameters =
        [
            new SqlParameter("@PermissionId", SqlDbType.Int) { Value = permissionId }
        ];

        var rows = await _dataAccess.QueryAsync<PermissionRecord>(
            PermissionQueries.GetById,
            parameters,
            cancellationToken);

        return rows.FirstOrDefault();
    }

    public async Task<bool> ExistsByCodeAsync(
        string permissionCode,
        int? excludePermissionId = null,
        CancellationToken cancellationToken = default)
    {
        SqlParameter[] parameters =
        [
            new SqlParameter("@PermissionCode", SqlDbType.NVarChar, 128) { Value = permissionCode },
            new SqlParameter("@ExcludePermissionId", SqlDbType.Int)
            {
                Value = excludePermissionId ?? (object)DBNull.Value
            }
        ];

        var count = await _dataAccess.ExecuteScalarAsync<int>(
            PermissionQueries.ExistsByCode,
            parameters,
            cancellationToken);

        return count > 0;
    }

    public async Task<int> GetAssignedRoleCountAsync(
        int permissionId,
        CancellationToken cancellationToken = default)
    {
        SqlParameter[] parameters =
        [
            new SqlParameter("@PermissionId", SqlDbType.Int) { Value = permissionId }
        ];

        return await _dataAccess.ExecuteScalarAsync<int>(
            PermissionQueries.GetAssignedRoleCount,
            parameters,
            cancellationToken);
    }

    public Task<List<PermissionRoleCountRow>> GetAssignedRoleCountsAsync(
        CancellationToken cancellationToken = default) =>
        _dataAccess.QueryAsync<PermissionRoleCountRow>(
            PermissionQueries.GetAssignedRoleCounts,
            null,
            cancellationToken);

    public Task<int> CreateAsync(
        PermissionRecord entity,
        CancellationToken cancellationToken = default) =>
        _dataAccess.ExecuteScalarAsync<int>(
            PermissionQueries.Create,
            CreateWriteParameters(entity),
            cancellationToken);

    public Task<int> UpdateAsync(
        PermissionRecord entity,
        CancellationToken cancellationToken = default)
    {
        var parameters = CreateWriteParameters(entity).ToList();
        parameters.Add(new SqlParameter("@PermissionId", SqlDbType.Int) { Value = entity.PermissionId });
        return _dataAccess.ExecuteAsync(PermissionQueries.Update, parameters, cancellationToken);
    }

    public Task<int> DeleteAsync(int permissionId, CancellationToken cancellationToken = default)
    {
        SqlParameter[] parameters =
        [
            new SqlParameter("@PermissionId", SqlDbType.Int) { Value = permissionId }
        ];

        return _dataAccess.ExecuteAsync(PermissionQueries.Delete, parameters, cancellationToken);
    }

    public async Task<int> CountExistingIdsAsync(
        IReadOnlyList<int> permissionIds,
        CancellationToken cancellationToken = default)
    {
        if (permissionIds.Count == 0)
        {
            return 0;
        }

        var placeholders = string.Join(", ", permissionIds.Select((_, index) => $"@P{index}"));
        var sql = string.Format(PermissionQueries.GetByIdsTemplate, placeholders);

        var parameters = permissionIds
            .Select((id, index) => new SqlParameter($"@P{index}", id))
            .ToArray();

        var rows = await _dataAccess.QueryAsync<PermissionIdRow>(sql, parameters, cancellationToken);
        return rows.Count;
    }

    private static SqlParameter[] CreateWriteParameters(PermissionRecord entity) =>
    [
        new SqlParameter("@PermissionCode", SqlDbType.NVarChar, 128) { Value = entity.PermissionCode },
        new SqlParameter("@Description", SqlDbType.NVarChar, 256)
        {
            Value = entity.Description ?? (object)DBNull.Value
        }
    ];

    private sealed class PermissionIdRow
    {
        public int PermissionId { get; set; }
    }
}
