using System.Data;
using Infrastructure.Database.DataAccess;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Repositories;

/// <summary>
/// Legacy dual-write target. Canonical sources are user_roles + user_company_access.
/// Do not use this table as the authorization source of truth.
/// </summary>
public sealed class UserCompanyRoleRepository
{
    private readonly ISqlDataAccess _dataAccess;

    public UserCompanyRoleRepository(ISqlDataAccess dataAccess)
    {
        _dataAccess = dataAccess;
    }

    public Task CreateAsync(
        int userId,
        int companyId,
        int roleId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO user_company_roles (user_id, company_id, role_id, is_active, created_at)
            VALUES (@UserId, @CompanyId, @RoleId, 1, SYSUTCDATETIME());
            """;

        SqlParameter[] parameters =
        [
            new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
            new SqlParameter("@CompanyId", SqlDbType.Int) { Value = companyId },
            new SqlParameter("@RoleId", SqlDbType.Int) { Value = roleId }
        ];

        return _dataAccess.ExecuteAsync(sql, parameters, cancellationToken);
    }

    public async Task<List<int>> GetCompanyIdsForUserAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT DISTINCT company_id AS CompanyId
            FROM user_company_roles
            WHERE user_id = @UserId
              AND is_active = 1;
            """;

        var rows = await _dataAccess.QueryAsync<CompanyIdRow>(
            sql,
            [new SqlParameter("@UserId", SqlDbType.Int) { Value = userId }],
            cancellationToken);

        return rows.Select(r => r.CompanyId).ToList();
    }

    public Task ReplaceForUserCompanyAsync(
        int userId,
        int companyId,
        IReadOnlyCollection<int> roleIds,
        CancellationToken cancellationToken = default) =>
        _dataAccess.ExecuteInTransactionAsync(async (connection, transaction, token) =>
        {
            await SqlDataAccess.ExecuteOnAsync(
                connection,
                transaction,
                """
                DELETE FROM user_company_roles
                WHERE user_id = @UserId
                  AND company_id = @CompanyId;
                """,
                [
                    new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
                    new SqlParameter("@CompanyId", SqlDbType.Int) { Value = companyId }
                ],
                token);

            foreach (var roleId in roleIds.Distinct())
            {
                await SqlDataAccess.ExecuteOnAsync(
                    connection,
                    transaction,
                    """
                    INSERT INTO user_company_roles (user_id, company_id, role_id, is_active, created_at)
                    VALUES (@UserId, @CompanyId, @RoleId, 1, SYSUTCDATETIME());
                    """,
                    [
                        new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
                        new SqlParameter("@CompanyId", SqlDbType.Int) { Value = companyId },
                        new SqlParameter("@RoleId", SqlDbType.Int) { Value = roleId }
                    ],
                    token);
            }
        }, cancellationToken);

    /// <summary>
    /// Full dual-write sync: replace all legacy UCR rows for the user with the
    /// cartesian product of companyIds × roleIds inside one transaction.
    /// </summary>
    public Task ReplaceAllForUserAsync(
        int userId,
        IReadOnlyCollection<int> companyIds,
        IReadOnlyCollection<int> roleIds,
        CancellationToken cancellationToken = default) =>
        _dataAccess.ExecuteInTransactionAsync(async (connection, transaction, token) =>
        {
            await SqlDataAccess.ExecuteOnAsync(
                connection,
                transaction,
                """
                DELETE FROM user_company_roles
                WHERE user_id = @UserId;
                """,
                [new SqlParameter("@UserId", SqlDbType.Int) { Value = userId }],
                token);

            foreach (var companyId in companyIds.Distinct())
            {
                foreach (var roleId in roleIds.Distinct())
                {
                    await SqlDataAccess.ExecuteOnAsync(
                        connection,
                        transaction,
                        """
                        INSERT INTO user_company_roles (user_id, company_id, role_id, is_active, created_at)
                        VALUES (@UserId, @CompanyId, @RoleId, 1, SYSUTCDATETIME());
                        """,
                        [
                            new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
                            new SqlParameter("@CompanyId", SqlDbType.Int) { Value = companyId },
                            new SqlParameter("@RoleId", SqlDbType.Int) { Value = roleId }
                        ],
                        token);
                }
            }
        }, cancellationToken);

    public Task DeleteByUserIdAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            DELETE FROM user_company_roles
            WHERE user_id = @UserId;
            """;

        return _dataAccess.ExecuteAsync(
            sql,
            [new SqlParameter("@UserId", SqlDbType.Int) { Value = userId }],
            cancellationToken);
    }
}
