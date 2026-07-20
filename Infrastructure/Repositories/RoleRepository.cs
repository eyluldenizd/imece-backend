using System.Data;
using Infrastructure.Database.DataAccess;
using Infrastructure.Entities;
using Infrastructure.Repositories.Queries;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Repositories;

public sealed class RoleRepository
{
    private readonly ISqlDataAccess _dataAccess;

    public RoleRepository(ISqlDataAccess dataAccess)
    {
        _dataAccess = dataAccess;
    }

    public Task<List<Roles>> GetAllAsync(CancellationToken cancellationToken = default) =>
        _dataAccess.QueryAsync<Roles>(RoleQueries.GetAll, null, cancellationToken);

    public Task<Roles?> GetByIdAsync(int roleId, CancellationToken cancellationToken = default)
    {
        SqlParameter[] parameters =
        [
            new SqlParameter("@RoleId", SqlDbType.Int) { Value = roleId }
        ];

        return _dataAccess.QueryFirstOrDefaultAsync<Roles>(
            RoleQueries.GetById,
            parameters,
            cancellationToken);
    }

    public Task<List<PermissionCodeRow>> GetPermissionCodesByRoleIdAsync(
        int roleId,
        CancellationToken cancellationToken = default)
    {
        SqlParameter[] parameters =
        [
            new SqlParameter("@RoleId", SqlDbType.Int) { Value = roleId }
        ];

        return _dataAccess.QueryAsync<PermissionCodeRow>(
            RoleQueries.GetPermissionCodesByRoleId,
            parameters,
            cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(
        string roleName,
        int? excludeRoleId = null,
        CancellationToken cancellationToken = default)
    {
        SqlParameter[] parameters =
        [
            new SqlParameter("@RoleName", SqlDbType.NVarChar, 64) { Value = roleName },
            new SqlParameter("@ExcludeRoleId", SqlDbType.Int)
            {
                Value = excludeRoleId.HasValue ? excludeRoleId.Value : DBNull.Value
            }
        ];

        var count = await _dataAccess.ExecuteScalarAsync<int>(
            RoleQueries.ExistsByName,
            parameters,
            cancellationToken);

        return count > 0;
    }

    public Task<int> CreateAsync(Roles entity, CancellationToken cancellationToken = default) =>
        _dataAccess.ExecuteScalarAsync<int>(
            RoleQueries.Create,
            CreateWriteParameters(entity),
            cancellationToken);

    public Task<int> UpdateAsync(Roles entity, CancellationToken cancellationToken = default)
    {
        var parameters = CreateWriteParameters(entity).ToList();
        parameters.Add(new SqlParameter("@RoleId", SqlDbType.Int) { Value = entity.RoleId });
        return _dataAccess.ExecuteAsync(RoleQueries.Update, parameters, cancellationToken);
    }

    public Task<int> SoftDeleteAsync(int roleId, CancellationToken cancellationToken = default)
    {
        SqlParameter[] parameters =
        [
            new SqlParameter("@RoleId", SqlDbType.Int) { Value = roleId }
        ];

        return _dataAccess.ExecuteAsync(RoleQueries.SoftDelete, parameters, cancellationToken);
    }

    public async Task ReplacePermissionsAsync(
        int roleId,
        IReadOnlyList<int> permissionIds,
        CancellationToken cancellationToken = default)
    {
        SqlParameter[] deleteParameters =
        [
            new SqlParameter("@RoleId", SqlDbType.Int) { Value = roleId }
        ];

        await _dataAccess.ExecuteAsync(
            RoleQueries.DeleteRolePermissions,
            deleteParameters,
            cancellationToken);

        foreach (var permissionId in permissionIds.Distinct())
        {
            SqlParameter[] insertParameters =
            [
                new SqlParameter("@RoleId", SqlDbType.Int) { Value = roleId },
                new SqlParameter("@PermissionId", SqlDbType.Int) { Value = permissionId }
            ];

            await _dataAccess.ExecuteAsync(
                RoleQueries.InsertRolePermission,
                insertParameters,
                cancellationToken);
        }
    }

    private static SqlParameter[] CreateWriteParameters(Roles entity) =>
    [
        new SqlParameter("@RoleName", SqlDbType.NVarChar, 64) { Value = entity.RoleName },
        new SqlParameter("@Description", SqlDbType.NVarChar, 256)
        {
            Value = entity.Description ?? (object)DBNull.Value
        },
        new SqlParameter("@IsActive", SqlDbType.Bit) { Value = entity.IsActive }
    ];

    public sealed class PermissionCodeRow
    {
        public string PermissionCode { get; set; } = string.Empty;
    }
}
