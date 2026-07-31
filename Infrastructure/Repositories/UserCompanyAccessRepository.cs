using System.Data;
using Infrastructure.Database.DataAccess;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Repositories;

public sealed class UserCompanyAccessRecord
{
    public long UserCompanyAccessId { get; set; }

    public int UserId { get; set; }

    public int CompanyId { get; set; }

    public string CompanyName { get; set; } = string.Empty;

    public string? CompanyCode { get; set; }

    /// <summary>Assignment is_active.</summary>
    public bool IsActive { get; set; }

    /// <summary>companies.is_active.</summary>
    public bool CompanyIsActive { get; set; }
}

public sealed class CompanyIdRow
{
    public int CompanyId { get; set; }
}

public sealed class UserCompanyAccessRepository
{
    private readonly ISqlDataAccess _dataAccess;

    public UserCompanyAccessRepository(ISqlDataAccess dataAccess)
    {
        _dataAccess = dataAccess;
    }

    public Task<List<UserCompanyAccessRecord>> GetByUserIdAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                uca.user_company_access_id AS UserCompanyAccessId,
                uca.user_id AS UserId,
                uca.company_id AS CompanyId,
                c.company_name AS CompanyName,
                c.company_code AS CompanyCode,
                uca.is_active AS IsActive,
                c.is_active AS CompanyIsActive
            FROM user_company_access uca
            INNER JOIN companies c ON c.company_id = uca.company_id
            WHERE uca.user_id = @UserId
              AND uca.is_active = 1
            ORDER BY c.company_name;
            """;

        return _dataAccess.QueryAsync<UserCompanyAccessRecord>(
            sql,
            [new SqlParameter("@UserId", SqlDbType.Int) { Value = userId }],
            cancellationToken);
    }

    public async Task<List<int>> GetCompanyIdsForUserAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT DISTINCT company_id AS CompanyId
            FROM user_company_access
            WHERE user_id = @UserId
              AND is_active = 1;
            """;

        var rows = await _dataAccess.QueryAsync<CompanyIdRow>(
            sql,
            [new SqlParameter("@UserId", SqlDbType.Int) { Value = userId }],
            cancellationToken);

        return rows.Select(r => r.CompanyId).ToList();
    }

    public Task DeleteByUserIdAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            DELETE FROM user_company_access
            WHERE user_id = @UserId;
            """;

        return _dataAccess.ExecuteAsync(
            sql,
            [new SqlParameter("@UserId", SqlDbType.Int) { Value = userId }],
            cancellationToken);
    }

    public Task InsertAsync(
        int userId,
        int companyId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            IF NOT EXISTS (
                SELECT 1 FROM user_company_access
                WHERE user_id = @UserId AND company_id = @CompanyId)
            BEGIN
                INSERT INTO user_company_access (user_id, company_id, is_active, created_at)
                VALUES (@UserId, @CompanyId, 1, SYSUTCDATETIME());
            END
            ELSE
            BEGIN
                UPDATE user_company_access
                SET is_active = 1
                WHERE user_id = @UserId AND company_id = @CompanyId;
            END
            """;

        return _dataAccess.ExecuteAsync(
            sql,
            [
                new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
                new SqlParameter("@CompanyId", SqlDbType.Int) { Value = companyId }
            ],
            cancellationToken);
    }

    public Task ReplaceAsync(
        int userId,
        IReadOnlyCollection<int> companyIds,
        CancellationToken cancellationToken = default) =>
        _dataAccess.ExecuteInTransactionAsync(async (connection, transaction, token) =>
        {
            await SqlDataAccess.ExecuteOnAsync(
                connection,
                transaction,
                """
                DELETE FROM user_company_access
                WHERE user_id = @UserId;
                """,
                [new SqlParameter("@UserId", SqlDbType.Int) { Value = userId }],
                token);

            foreach (var companyId in companyIds.Distinct())
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
        }, cancellationToken);
}
