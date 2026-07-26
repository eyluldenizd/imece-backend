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
        await SeedPermissionsAsync(connection, transaction, commandTimeoutSeconds, cancellationToken);
        await SeedSystemRolesAsync(connection, transaction, commandTimeoutSeconds, cancellationToken);
        await PurgeNonSystemRolesAsync(connection, transaction, commandTimeoutSeconds, cancellationToken);
        await SyncSystemRolePermissionsAsync(connection, transaction, commandTimeoutSeconds, cancellationToken);
        await SeedDefaultCompanyAsync(connection, transaction, commandTimeoutSeconds, cancellationToken);
        await SeedDishCategoriesAsync(connection, transaction, commandTimeoutSeconds, cancellationToken);
        await BackfillDishCategoriesAsync(connection, transaction, commandTimeoutSeconds, cancellationToken);
        await SeedCommunicationChannelTypesAsync(connection, transaction, commandTimeoutSeconds, cancellationToken);
        await SeedCorporateAppCategoriesAsync(connection, transaction, commandTimeoutSeconds, cancellationToken);
        await SeedServiceLocationTypesAsync(connection, transaction, commandTimeoutSeconds, cancellationToken);
        _logger.LogInformation("Sistem seed verileri uygulandı.");
    }

    private async Task SeedSystemRolesAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        int timeout,
        CancellationToken cancellationToken)
    {
        foreach (var role in SystemRoleCatalog.All)
        {
            await _executor.ExecuteNonQueryAsync(
                connection,
                """
                IF NOT EXISTS (SELECT 1 FROM [dbo].[roles] WHERE role_name = @RoleName)
                BEGIN
                    INSERT INTO [dbo].[roles] (role_name, description, is_active)
                    VALUES (@RoleName, @Description, 1);
                END
                ELSE
                BEGIN
                    UPDATE [dbo].[roles]
                    SET description = @Description,
                        is_active = 1
                    WHERE role_name = @RoleName;
                END
                """,
                parameters:
                [
                    new SqlParameter("@RoleName", role.Code),
                    new SqlParameter("@Description", role.Description)
                ],
                transaction: transaction,
                commandTimeoutSeconds: timeout,
                cancellationToken: cancellationToken);
        }
    }

    private async Task PurgeNonSystemRolesAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        int timeout,
        CancellationToken cancellationToken)
    {
        var allowedRolesSql = string.Join(
            ", ",
            SystemRoleCatalog.All.Select(role => $"N'{role.Code.Replace("'", "''")}'"));

        await _executor.ExecuteNonQueryAsync(
            connection,
            $"""
             DECLARE @UserRoleId INT = (
                 SELECT TOP 1 role_id
                 FROM [dbo].[roles]
                 WHERE role_name = N'{Roles.User.Replace("'", "''")}'
             );

             IF @UserRoleId IS NOT NULL
             BEGIN
                 UPDATE u
                 SET u.role_id = @UserRoleId
                 FROM [dbo].[users] AS u
                 INNER JOIN [dbo].[roles] AS r ON r.role_id = u.role_id
                 WHERE r.role_name NOT IN ({allowedRolesSql});

                 DELETE ucr
                 FROM [dbo].[user_company_roles] AS ucr
                 INNER JOIN [dbo].[roles] AS deprecated ON deprecated.role_id = ucr.role_id
                 INNER JOIN [dbo].[user_company_roles] AS existing
                     ON existing.user_id = ucr.user_id
                    AND existing.company_id = ucr.company_id
                 INNER JOIN [dbo].[roles] AS fallback ON fallback.role_id = existing.role_id
                 WHERE deprecated.role_name NOT IN ({allowedRolesSql})
                   AND fallback.role_name = N'{Roles.User.Replace("'", "''")}'
                   AND existing.user_company_role_id <> ucr.user_company_role_id;

                 UPDATE ucr
                 SET ucr.role_id = @UserRoleId
                 FROM [dbo].[user_company_roles] AS ucr
                 INNER JOIN [dbo].[roles] AS r ON r.role_id = ucr.role_id
                 WHERE r.role_name NOT IN ({allowedRolesSql});
             END

             DELETE rp
             FROM [dbo].[role_permissions] AS rp
             INNER JOIN [dbo].[roles] AS r ON r.role_id = rp.role_id
             WHERE r.role_name NOT IN ({allowedRolesSql});

             DELETE FROM [dbo].[roles]
             WHERE role_name NOT IN ({allowedRolesSql});
             """,
            transaction: transaction,
            commandTimeoutSeconds: timeout,
            cancellationToken: cancellationToken);

        _logger.LogInformation("Sistem dışı roller temizlendi.");
    }

    private async Task SyncSystemRolePermissionsAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        int timeout,
        CancellationToken cancellationToken)
    {
        foreach (var role in SystemRoleCatalog.All)
        {
            await _executor.ExecuteNonQueryAsync(
                connection,
                """
                DELETE rp
                FROM [dbo].[role_permissions] AS rp
                INNER JOIN [dbo].[roles] AS r ON r.role_id = rp.role_id
                WHERE r.role_name = @RoleName;
                """,
                parameters: [new SqlParameter("@RoleName", role.Code)],
                transaction: transaction,
                commandTimeoutSeconds: timeout,
                cancellationToken: cancellationToken);

            foreach (var permissionCode in role.Permissions)
            {
                await _executor.ExecuteNonQueryAsync(
                    connection,
                    """
                    INSERT INTO [dbo].[role_permissions] (role_id, permission_id)
                    SELECT r.role_id, p.permission_id
                    FROM [dbo].[roles] AS r
                    CROSS JOIN [dbo].[permissions] AS p
                    WHERE r.role_name = @RoleName
                      AND p.permission_code = @PermissionCode;
                    """,
                    parameters:
                    [
                        new SqlParameter("@RoleName", role.Code),
                        new SqlParameter("@PermissionCode", permissionCode)
                    ],
                    transaction: transaction,
                    commandTimeoutSeconds: timeout,
                    cancellationToken: cancellationToken);
            }
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
            (Permissions.AdminPanelAccess, "Admin panel erişimi"),
            (Permissions.ContentGlobalManage, "Global içerik yönetimi"),
            (Permissions.ContentCompanyManage, "Şirket içerik yönetimi"),
            (Permissions.MediaManage, "Medya yönetimi"),
            (Permissions.UsersManage, "Kullanıcı yönetimi"),
            (Permissions.MenusManage, "Menü yönetimi"),
            (Permissions.PermissionsManage, "Yetki ve rol atamalarını düzenleme")
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
                ELSE
                BEGIN
                    UPDATE [dbo].[permissions]
                    SET description = @Description
                    WHERE permission_code = @Code;
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

    private async Task SeedDishCategoriesAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        int timeout,
        CancellationToken cancellationToken)
    {
        var categories = new (string Name, string Code, int SortOrder)[]
        {
            ("Çorba", "corba", 1),
            ("Ana Yemek", "ana-yemek", 2),
            ("Yardımcı Yemek", "yardimci-yemek", 3),
            ("Salata", "salata", 4),
            ("Tatlı", "tatli", 5),
            ("İçecek", "icecek", 6),
            ("Diğer", "diger", 7)
        };

        foreach (var (name, code, sortOrder) in categories)
        {
            await _executor.ExecuteNonQueryAsync(
                connection,
                """
                IF NOT EXISTS (SELECT 1 FROM [dbo].[dish_categories] WHERE code = @Code)
                BEGIN
                    INSERT INTO [dbo].[dish_categories] (name, code, sort_order, is_active, created_at, updated_at)
                    VALUES (@Name, @Code, @SortOrder, 1, SYSUTCDATETIME(), SYSUTCDATETIME());
                END
                """,
                parameters:
                [
                    new SqlParameter("@Name", name),
                    new SqlParameter("@Code", code),
                    new SqlParameter("@SortOrder", sortOrder)
                ],
                transaction: transaction,
                commandTimeoutSeconds: timeout,
                cancellationToken: cancellationToken);
        }
    }

    private async Task BackfillDishCategoriesAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        int timeout,
        CancellationToken cancellationToken)
    {
        await _executor.ExecuteNonQueryAsync(
            connection,
            """
            UPDATE d
            SET d.dish_category_id = COALESCE(
                (
                    SELECT TOP 1 dc.dish_category_id
                    FROM [dbo].[dish_categories] AS dc
                    WHERE dc.is_active = 1
                      AND (
                          LTRIM(RTRIM(LOWER(d.category))) = LTRIM(RTRIM(LOWER(dc.name)))
                          OR LTRIM(RTRIM(LOWER(d.category))) = LTRIM(RTRIM(LOWER(dc.code)))
                      )
                    ORDER BY dc.sort_order
                ),
                (
                    SELECT TOP 1 dc.dish_category_id
                    FROM [dbo].[dish_categories] AS dc
                    WHERE dc.code = N'diger'
                )
            )
            FROM [dbo].[dishes] AS d
            WHERE d.dish_category_id IS NULL;
            """,
            transaction: transaction,
            commandTimeoutSeconds: timeout,
            cancellationToken: cancellationToken);
    }

    private async Task SeedCommunicationChannelTypesAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        int timeout,
        CancellationToken cancellationToken)
    {
        var types = new (string Name, string Code, int SortOrder)[]
        {
            ("Instagram", "instagram", 1),
            ("LinkedIn", "linkedin", 2),
            ("YouTube", "youtube", 3),
            ("X", "x", 4),
            ("Facebook", "facebook", 5),
            ("WhatsApp", "whatsapp", 6),
            ("Email", "email", 7),
            ("Phone", "phone", 8),
            ("Web", "web", 9)
        };

        foreach (var (name, code, sortOrder) in types)
        {
            await _executor.ExecuteNonQueryAsync(
                connection,
                """
                IF NOT EXISTS (SELECT 1 FROM [dbo].[communication_channel_types] WHERE code = @Code)
                BEGIN
                    INSERT INTO [dbo].[communication_channel_types]
                        (name, code, sort_order, is_active, created_at, updated_at)
                    VALUES (@Name, @Code, @SortOrder, 1, SYSUTCDATETIME(), SYSUTCDATETIME());
                END
                """,
                parameters:
                [
                    new SqlParameter("@Name", name),
                    new SqlParameter("@Code", code),
                    new SqlParameter("@SortOrder", sortOrder)
                ],
                transaction: transaction,
                commandTimeoutSeconds: timeout,
                cancellationToken: cancellationToken);
        }
    }

    private async Task SeedCorporateAppCategoriesAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        int timeout,
        CancellationToken cancellationToken)
    {
        var categories = new (string Name, int SortOrder)[]
        {
            ("HR", 1),
            ("IT", 2),
            ("Finance", 3),
            ("Operations", 4)
        };

        foreach (var (name, sortOrder) in categories)
        {
            await _executor.ExecuteNonQueryAsync(
                connection,
                """
                IF NOT EXISTS (SELECT 1 FROM [dbo].[corporate_app_categories] WHERE name = @Name)
                BEGIN
                    INSERT INTO [dbo].[corporate_app_categories]
                        (name, sort_order, is_active, created_at, updated_at)
                    VALUES (@Name, @SortOrder, 1, SYSUTCDATETIME(), SYSUTCDATETIME());
                END
                """,
                parameters:
                [
                    new SqlParameter("@Name", name),
                    new SqlParameter("@SortOrder", sortOrder)
                ],
                transaction: transaction,
                commandTimeoutSeconds: timeout,
                cancellationToken: cancellationToken);
        }
    }

    private async Task SeedServiceLocationTypesAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        int timeout,
        CancellationToken cancellationToken)
    {
        var types = new (string Name, int SortOrder)[]
        {
            ("Durak", 1),
            ("Merkez", 2),
            ("Fabrika", 3),
            ("Ofis", 4),
            ("Terminal", 5),
            ("Aktarma Noktası", 6)
        };

        foreach (var (name, sortOrder) in types)
        {
            await _executor.ExecuteNonQueryAsync(
                connection,
                """
                IF NOT EXISTS (SELECT 1 FROM [dbo].[service_location_types] WHERE name = @Name)
                BEGIN
                    INSERT INTO [dbo].[service_location_types]
                        (name, sort_order, is_active, created_at, updated_at)
                    VALUES (@Name, @SortOrder, 1, SYSUTCDATETIME(), SYSUTCDATETIME());
                END
                """,
                parameters:
                [
                    new SqlParameter("@Name", name),
                    new SqlParameter("@SortOrder", sortOrder)
                ],
                transaction: transaction,
                commandTimeoutSeconds: timeout,
                cancellationToken: cancellationToken);
        }
    }
}
