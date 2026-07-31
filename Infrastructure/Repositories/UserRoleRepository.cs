using System.Data;
using Infrastructure.Database.DataAccess;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Repositories;

public sealed class UserRoleAssignmentRecord
{
    public long UserRoleId { get; set; }

    public int UserId { get; set; }

    public int RoleId { get; set; }

    public string RoleName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; }
}

public sealed class UserRoleRepository
{
    private readonly ISqlDataAccess _dataAccess;

    public UserRoleRepository(ISqlDataAccess dataAccess)
    {
        _dataAccess = dataAccess;
    }

    public Task<List<UserRoleAssignmentRecord>> GetByUserIdAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                ur.user_role_id AS UserRoleId,
                ur.user_id AS UserId,
                ur.role_id AS RoleId,
                r.role_name AS RoleName,
                r.description AS Description,
                ur.is_active AS IsActive
            FROM user_roles ur
            INNER JOIN roles r ON r.role_id = ur.role_id
            WHERE ur.user_id = @UserId
              AND ur.is_active = 1
            ORDER BY r.role_name;
            """;

        return _dataAccess.QueryAsync<UserRoleAssignmentRecord>(
            sql,
            [new SqlParameter("@UserId", SqlDbType.Int) { Value = userId }],
            cancellationToken);
    }

    public Task<int> CountUsersWithRoleAsync(
        int roleId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT COUNT(1)
            FROM user_roles
            WHERE role_id = @RoleId
              AND is_active = 1;
            """;

        return _dataAccess.ExecuteScalarAsync<int>(
            sql,
            [new SqlParameter("@RoleId", SqlDbType.Int) { Value = roleId }],
            cancellationToken)!;
    }

    public Task DeleteByUserIdAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            DELETE FROM user_roles
            WHERE user_id = @UserId;
            """;

        return _dataAccess.ExecuteAsync(
            sql,
            [new SqlParameter("@UserId", SqlDbType.Int) { Value = userId }],
            cancellationToken);
    }

    public Task InsertAsync(
        int userId,
        int roleId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            IF NOT EXISTS (
                SELECT 1 FROM user_roles
                WHERE user_id = @UserId AND role_id = @RoleId)
            BEGIN
                INSERT INTO user_roles (user_id, role_id, is_active, created_at)
                VALUES (@UserId, @RoleId, 1, SYSUTCDATETIME());
            END
            ELSE
            BEGIN
                UPDATE user_roles
                SET is_active = 1
                WHERE user_id = @UserId AND role_id = @RoleId;
            END
            """;

        return _dataAccess.ExecuteAsync(
            sql,
            [
                new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
                new SqlParameter("@RoleId", SqlDbType.Int) { Value = roleId }
            ],
            cancellationToken);
    }

    public Task ReplaceAsync(
        int userId,
        IReadOnlyCollection<int> roleIds,
        CancellationToken cancellationToken = default) =>
        _dataAccess.ExecuteInTransactionAsync(async (connection, transaction, token) =>
        {
            await SqlDataAccess.ExecuteOnAsync(
                connection,
                transaction,
                """
                DELETE FROM user_roles
                WHERE user_id = @UserId;
                """,
                [new SqlParameter("@UserId", SqlDbType.Int) { Value = userId }],
                token);

            foreach (var roleId in roleIds.Distinct())
            {
                await SqlDataAccess.ExecuteOnAsync(
                    connection,
                    transaction,
                    """
                    INSERT INTO user_roles (user_id, role_id, is_active, created_at)
                    VALUES (@UserId, @RoleId, 1, SYSUTCDATETIME());
                    """,
                    [
                        new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
                        new SqlParameter("@RoleId", SqlDbType.Int) { Value = roleId }
                    ],
                    token);
            }
        }, cancellationToken);
}
