using System.Data;
using Infrastructure.Database.DataAccess;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Repositories;

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
}
