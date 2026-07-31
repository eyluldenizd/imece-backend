using System.Data;
using Infrastructure.Database.DataAccess;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Repositories;

/// <summary>
/// Canonical + legacy dual-write for user role/company assignments in one transaction.
/// Source of truth: user_roles / user_company_access.
/// Compatibility: users.role_id / user_company_roles.
/// </summary>
public sealed class UserAccessWriteRepository
{
    private readonly ISqlDataAccess _dataAccess;

    public UserAccessWriteRepository(ISqlDataAccess dataAccess)
    {
        _dataAccess = dataAccess;
    }

    public Task ReplaceRolesAsync(
        int userId,
        IReadOnlyList<int> roleIds,
        IReadOnlyList<int> companyIdsForLegacy,
        CancellationToken cancellationToken = default) =>
        _dataAccess.ExecuteInTransactionAsync(async (connection, transaction, token) =>
        {
            var distinctRoles = roleIds.Distinct().ToArray();
            if (distinctRoles.Length == 0)
            {
                throw new InvalidOperationException("En az bir rol gerekir.");
            }

            await SqlDataAccess.ExecuteOnAsync(
                connection,
                transaction,
                "DELETE FROM user_roles WHERE user_id = @UserId;",
                [new SqlParameter("@UserId", SqlDbType.Int) { Value = userId }],
                token);

            foreach (var roleId in distinctRoles)
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

            // Legacy primary role
            await SqlDataAccess.ExecuteOnAsync(
                connection,
                transaction,
                "UPDATE users SET role_id = @RoleId WHERE user_id = @UserId;",
                [
                    new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
                    new SqlParameter("@RoleId", SqlDbType.Int) { Value = distinctRoles[0] }
                ],
                token);

            await ReplaceLegacyUcrOnAsync(
                connection,
                transaction,
                userId,
                companyIdsForLegacy,
                distinctRoles,
                token);
        }, cancellationToken);

    public Task ReplaceCompaniesAsync(
        int userId,
        IReadOnlyList<int> companyIds,
        IReadOnlyList<int> roleIdsForLegacy,
        CancellationToken cancellationToken = default) =>
        _dataAccess.ExecuteInTransactionAsync(async (connection, transaction, token) =>
        {
            var distinctCompanies = companyIds.Distinct().ToArray();
            var distinctRoles = roleIdsForLegacy.Distinct().ToArray();

            await SqlDataAccess.ExecuteOnAsync(
                connection,
                transaction,
                "DELETE FROM user_company_access WHERE user_id = @UserId;",
                [new SqlParameter("@UserId", SqlDbType.Int) { Value = userId }],
                token);

            foreach (var companyId in distinctCompanies)
            {
                await SqlDataAccess.ExecuteOnAsync(
                    connection,
                    transaction,
                    """
                    INSERT INTO user_company_access (user_id, company_id, is_active, created_at)
                    VALUES (@UserId, @CompanyId, 1, SYSUTCDATETIME());
                    """,
                    [
                        new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
                        new SqlParameter("@CompanyId", SqlDbType.Int) { Value = companyId }
                    ],
                    token);
            }

            await ReplaceLegacyUcrOnAsync(
                connection,
                transaction,
                userId,
                distinctCompanies,
                distinctRoles,
                token);
        }, cancellationToken);

    private static async Task ReplaceLegacyUcrOnAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        int userId,
        IReadOnlyList<int> companyIds,
        IReadOnlyList<int> roleIds,
        CancellationToken cancellationToken)
    {
        await SqlDataAccess.ExecuteOnAsync(
            connection,
            transaction,
            "DELETE FROM user_company_roles WHERE user_id = @UserId;",
            [new SqlParameter("@UserId", SqlDbType.Int) { Value = userId }],
            cancellationToken);

        if (roleIds.Count == 0)
        {
            return;
        }

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
                    cancellationToken);
            }
        }
    }
}
