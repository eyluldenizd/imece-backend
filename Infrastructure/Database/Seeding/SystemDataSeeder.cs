using Core.Authorization;
using Infrastructure.Database.Connections;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Database.Seeding;

public sealed class SystemDataSeeder : ISystemDataSeeder
{
    private readonly IDbExecutor _executor;
    private readonly ILogger<SystemDataSeeder> _logger;

    public SystemDataSeeder(IDbExecutor executor, ILogger<SystemDataSeeder> logger)
    {
        _executor = executor;
        _logger = logger;
    }

    public async Task SeedAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        int commandTimeoutSeconds,
        CancellationToken cancellationToken = default)
    {
        await SeedRolesAsync(connection, transaction, commandTimeoutSeconds, cancellationToken);
        await SeedPermissionsAsync(connection, transaction, commandTimeoutSeconds, cancellationToken);
        await SeedRolePermissionsAsync(connection, transaction, commandTimeoutSeconds, cancellationToken);
        await SeedDefaultCompanyAsync(connection, transaction, commandTimeoutSeconds, cancellationToken);
        _logger.LogInformation("Sistem seed verileri uygulandı.");
    }

    private async Task SeedRolesAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        int timeout,
        CancellationToken cancellationToken)
    {
        var roles = new (string Name, string Description)[]
        {
            (Roles.User, "Standart kullanıcı"),
            (Roles.Editor, "İçerik editörü"),
            (Roles.CompanyAdmin, "Şirket yöneticisi"),
            (Roles.GlobalAdmin, "Global yönetici")
        };

        foreach (var (name, description) in roles)
        {
            await _executor.ExecuteNonQueryAsync(
                connection,
                """
                IF NOT EXISTS (SELECT 1 FROM [dbo].[roles] WHERE role_name = @RoleName)
                BEGIN
                    INSERT INTO [dbo].[roles] (role_name, description)
                    VALUES (@RoleName, @Description);
                END
                """,
                parameters:
                [
                    new SqlParameter("@RoleName", name),
                    new SqlParameter("@Description", description)
                ],
                transaction: transaction,
                commandTimeoutSeconds: timeout,
                cancellationToken: cancellationToken);
        }
    }

    private async Task SeedPermissionsAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        int timeout,
        CancellationToken cancellationToken)
    {
        var permissions = new (string Code, string Description)[]
        {
            (Permissions.ContentGlobalManage, "Global içerik yönetimi"),
            (Permissions.ContentCompanyManage, "Şirket içerik yönetimi"),
            (Permissions.MediaManage, "Medya yönetimi"),
            (Permissions.UsersManage, "Kullanıcı yönetimi")
        };

        foreach (var (code, description) in permissions)
        {
            await _executor.ExecuteNonQueryAsync(
                connection,
                """
                IF NOT EXISTS (SELECT 1 FROM [dbo].[permissions] WHERE permission_code = @Code)
                BEGIN
                    INSERT INTO [dbo].[permissions] (permission_code, description)
                    VALUES (@Code, @Description);
                END
                """,
                parameters:
                [
                    new SqlParameter("@Code", code),
                    new SqlParameter("@Description", description)
                ],
                transaction: transaction,
                commandTimeoutSeconds: timeout,
                cancellationToken: cancellationToken);
        }
    }

    private async Task SeedRolePermissionsAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        int timeout,
        CancellationToken cancellationToken)
    {
        var mappings = new (string Role, string Permission)[]
        {
            (Roles.GlobalAdmin, Permissions.ContentGlobalManage),
            (Roles.GlobalAdmin, Permissions.ContentCompanyManage),
            (Roles.GlobalAdmin, Permissions.MediaManage),
            (Roles.GlobalAdmin, Permissions.UsersManage),
            (Roles.CompanyAdmin, Permissions.ContentCompanyManage),
            (Roles.CompanyAdmin, Permissions.MediaManage),
            (Roles.CompanyAdmin, Permissions.UsersManage),
            (Roles.Editor, Permissions.ContentCompanyManage),
            (Roles.Editor, Permissions.MediaManage)
        };

        foreach (var (role, permission) in mappings)
        {
            await _executor.ExecuteNonQueryAsync(
                connection,
                """
                IF NOT EXISTS (
                    SELECT 1
                    FROM [dbo].[role_permissions] AS rp
                    INNER JOIN [dbo].[roles] AS r ON r.role_id = rp.role_id
                    INNER JOIN [dbo].[permissions] AS p ON p.permission_id = rp.permission_id
                    WHERE r.role_name = @RoleName
                      AND p.permission_code = @PermissionCode
                )
                BEGIN
                    INSERT INTO [dbo].[role_permissions] (role_id, permission_id)
                    SELECT r.role_id, p.permission_id
                    FROM [dbo].[roles] AS r
                    CROSS JOIN [dbo].[permissions] AS p
                    WHERE r.role_name = @RoleName
                      AND p.permission_code = @PermissionCode;
                END
                """,
                parameters:
                [
                    new SqlParameter("@RoleName", role),
                    new SqlParameter("@PermissionCode", permission)
                ],
                transaction: transaction,
                commandTimeoutSeconds: timeout,
                cancellationToken: cancellationToken);
        }
    }

    private async Task SeedDefaultCompanyAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        int timeout,
        CancellationToken cancellationToken)
    {
        await _executor.ExecuteNonQueryAsync(
            connection,
            """
            IF NOT EXISTS (SELECT 1 FROM [dbo].[companies] WHERE company_code = N'DEFAULT')
            BEGIN
                INSERT INTO [dbo].[companies] (company_code, company_name, is_active, created_at)
                VALUES (N'DEFAULT', N'Default Company', 1, SYSUTCDATETIME());
            END
            """,
            transaction: transaction,
            commandTimeoutSeconds: timeout,
            cancellationToken: cancellationToken);
    }
}
