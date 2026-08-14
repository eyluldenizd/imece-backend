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
        // Custom roller korunur (Role CRUD ile uyum). Yalnızca sistem rolleri senkronize edilir.
        await SyncSystemRolePermissionsAsync(connection, transaction, commandTimeoutSeconds, cancellationToken);
        await BackfillSeparatedAccessTablesAsync(connection, transaction, commandTimeoutSeconds, cancellationToken);
        await SeedDefaultCompanyAsync(connection, transaction, commandTimeoutSeconds, cancellationToken);
        await SeedDishCategoriesAsync(connection, transaction, commandTimeoutSeconds, cancellationToken);
        await BackfillDishCategoriesAsync(connection, transaction, commandTimeoutSeconds, cancellationToken);
        await SeedCommunicationChannelTypesAsync(connection, transaction, commandTimeoutSeconds, cancellationToken);
        await SeedCorporateAppCategoriesAsync(connection, transaction, commandTimeoutSeconds, cancellationToken);
        await SeedEmergencyNumberCategoriesAsync(connection, transaction, commandTimeoutSeconds, cancellationToken);
        await BackfillEmergencyNumberCategoriesAsync(connection, transaction, commandTimeoutSeconds, cancellationToken);
        await SeedServiceLocationTypesAsync(connection, transaction, commandTimeoutSeconds, cancellationToken);
        await MigrateWeeklyMenusBranchIndexAsync(connection, transaction, commandTimeoutSeconds, cancellationToken);
        await MigrateServiceTransportOrganizationScopeAsync(connection, transaction, commandTimeoutSeconds, cancellationToken);
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
        foreach (var permission in SystemPermissionCatalog.All)
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
                    new SqlParameter("@Code", permission.Code),
                    new SqlParameter("@Description", permission.Description)
                ],
                transaction: transaction,
                commandTimeoutSeconds: timeout,
                cancellationToken: cancellationToken);
        }
    }

    /// <summary>
    /// Legacy <c>user_company_roles</c> / <c>users.role_id</c> kayıtlarından
    /// <c>user_roles</c> ve <c>user_company_access</c> tablolarına idempotent backfill.
    /// </summary>
    private async Task BackfillSeparatedAccessTablesAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        int timeout,
        CancellationToken cancellationToken)
    {
        await _executor.ExecuteNonQueryAsync(
            connection,
            """
            IF OBJECT_ID(N'[dbo].[user_roles]', N'U') IS NULL
               OR OBJECT_ID(N'[dbo].[user_company_access]', N'U') IS NULL
            BEGIN
                RETURN;
            END

            INSERT INTO [dbo].[user_roles] (user_id, role_id, is_active, created_at)
            SELECT DISTINCT ucr.user_id, ucr.role_id, 1, SYSUTCDATETIME()
            FROM [dbo].[user_company_roles] AS ucr
            WHERE ucr.is_active = 1
              AND NOT EXISTS (
                  SELECT 1 FROM [dbo].[user_roles] AS ur
                  WHERE ur.user_id = ucr.user_id AND ur.role_id = ucr.role_id);

            INSERT INTO [dbo].[user_roles] (user_id, role_id, is_active, created_at)
            SELECT u.user_id, u.role_id, 1, SYSUTCDATETIME()
            FROM [dbo].[users] AS u
            WHERE u.role_id IS NOT NULL
              AND NOT EXISTS (
                  SELECT 1 FROM [dbo].[user_roles] AS ur
                  WHERE ur.user_id = u.user_id AND ur.role_id = u.role_id);

            INSERT INTO [dbo].[user_company_access] (user_id, company_id, is_active, created_at)
            SELECT DISTINCT ucr.user_id, ucr.company_id, 1, SYSUTCDATETIME()
            FROM [dbo].[user_company_roles] AS ucr
            WHERE ucr.is_active = 1
              AND NOT EXISTS (
                  SELECT 1 FROM [dbo].[user_company_access] AS uca
                  WHERE uca.user_id = ucr.user_id AND uca.company_id = ucr.company_id);
            """,
            transaction: transaction,
            commandTimeoutSeconds: timeout,
            cancellationToken: cancellationToken);

        _logger.LogInformation(
            "user_roles / user_company_access backfill uygulandı (idempotent).");
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

    private async Task SeedEmergencyNumberCategoriesAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        int timeout,
        CancellationToken cancellationToken)
    {
        var categories = new (string Name, int SortOrder)[]
        {
            ("Acil", 1),
            ("Güvenlik", 2),
            ("Sağlık", 3),
            ("İSG", 4),
            ("Altyapı", 5),
            ("Tesis", 6),
            ("İK", 7),
            ("BT", 8),
            ("Hukuk", 9),
            ("İletişim", 10),
            ("Operasyon", 11),
            ("Resepsiyon", 12)
        };

        foreach (var (name, sortOrder) in categories)
        {
            await _executor.ExecuteNonQueryAsync(
                connection,
                """
                IF NOT EXISTS (SELECT 1 FROM [dbo].[emergency_number_categories] WHERE name = @Name)
                BEGIN
                    INSERT INTO [dbo].[emergency_number_categories]
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

        // Ensure any legacy denormalized category names exist as category rows.
        await _executor.ExecuteNonQueryAsync(
            connection,
            """
            INSERT INTO [dbo].[emergency_number_categories]
                (name, sort_order, is_active, created_at, updated_at)
            SELECT DISTINCT
                LTRIM(RTRIM(en.category)),
                100,
                1,
                SYSUTCDATETIME(),
                SYSUTCDATETIME()
            FROM [dbo].[emergency_numbers] AS en
            WHERE en.category IS NOT NULL
              AND LTRIM(RTRIM(en.category)) <> N''
              AND NOT EXISTS (
                  SELECT 1
                  FROM [dbo].[emergency_number_categories] AS c
                  WHERE LTRIM(RTRIM(LOWER(c.name))) = LTRIM(RTRIM(LOWER(en.category)))
              );
            """,
            transaction: transaction,
            commandTimeoutSeconds: timeout,
            cancellationToken: cancellationToken);
    }

    private async Task BackfillEmergencyNumberCategoriesAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        int timeout,
        CancellationToken cancellationToken)
    {
        await _executor.ExecuteNonQueryAsync(
            connection,
            """
            UPDATE en
            SET en.emergency_number_category_id = c.emergency_number_category_id,
                en.category = c.name
            FROM [dbo].[emergency_numbers] AS en
            INNER JOIN [dbo].[emergency_number_categories] AS c
                ON LTRIM(RTRIM(LOWER(en.category))) = LTRIM(RTRIM(LOWER(c.name)))
            WHERE en.emergency_number_category_id IS NULL
              AND en.category IS NOT NULL
              AND LTRIM(RTRIM(en.category)) <> N'';
            """,
            transaction: transaction,
            commandTimeoutSeconds: timeout,
            cancellationToken: cancellationToken);
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
                IF NOT EXISTS (
                    SELECT 1 FROM [dbo].[service_location_types]
                    WHERE name = @Name
                      AND (
                            company_id IS NULL
                         OR company_id = (SELECT TOP (1) company_id FROM [dbo].[companies] ORDER BY company_id)
                      )
                )
                BEGIN
                    INSERT INTO [dbo].[service_location_types]
                        (company_id, name, sort_order, is_active, created_at, updated_at)
                    VALUES (
                        (SELECT TOP (1) company_id FROM [dbo].[companies] ORDER BY company_id),
                        @Name,
                        @SortOrder,
                        1,
                        SYSUTCDATETIME(),
                        SYSUTCDATETIME());
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

    /// <summary>
    /// weekly_menus: drop company-only unique index so the same week can exist per branch.
    /// New unique index (company_id, branch_id, menu_code) is created by schema sync.
    /// </summary>
    private async Task MigrateWeeklyMenusBranchIndexAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        int timeout,
        CancellationToken cancellationToken)
    {
        await _executor.ExecuteNonQueryAsync(
            connection,
            """
            IF OBJECT_ID(N'[dbo].[weekly_menus]', N'U') IS NULL
                RETURN;

            IF EXISTS (
                SELECT 1 FROM sys.indexes
                WHERE name = N'UX_weekly_menus_company_menu_code'
                  AND object_id = OBJECT_ID(N'[dbo].[weekly_menus]')
            )
            BEGIN
                DROP INDEX [UX_weekly_menus_company_menu_code] ON [dbo].[weekly_menus];
            END
            """,
            transaction: transaction,
            commandTimeoutSeconds: timeout,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Servis Yönetimi: şirket/şube kapsam kolonlarını doldur, eski unique index'i kaldır.
    /// </summary>
    private async Task MigrateServiceTransportOrganizationScopeAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        int timeout,
        CancellationToken cancellationToken)
    {
        await _executor.ExecuteNonQueryAsync(
            connection,
            """
            IF OBJECT_ID(N'[dbo].[service_location_types]', N'U') IS NOT NULL
            BEGIN
                IF EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE name = N'UX_service_location_types_name'
                      AND object_id = OBJECT_ID(N'[dbo].[service_location_types]')
                )
                BEGIN
                    DROP INDEX [UX_service_location_types_name] ON [dbo].[service_location_types];
                END

                IF COL_LENGTH(N'dbo.service_location_types', N'company_id') IS NOT NULL
                BEGIN
                    UPDATE t
                    SET t.company_id = c.company_id
                    FROM [dbo].[service_location_types] AS t
                    CROSS APPLY (
                        SELECT TOP (1) company_id
                        FROM [dbo].[companies]
                        ORDER BY company_id
                    ) AS c
                    WHERE t.company_id IS NULL;
                END

                IF COL_LENGTH(N'dbo.service_location_types', N'company_scope') IS NOT NULL
                BEGIN
                    UPDATE [dbo].[service_location_types]
                    SET company_scope = N'Specific'
                    WHERE company_id IS NOT NULL;

                    UPDATE [dbo].[service_location_types]
                    SET company_scope = N'All'
                    WHERE company_id IS NULL;
                END

                IF COL_LENGTH(N'dbo.service_location_types', N'branch_scope') IS NOT NULL
                BEGIN
                    UPDATE [dbo].[service_location_types]
                    SET branch_scope = N'All'
                    WHERE branch_scope IS NULL;
                END

                IF COL_LENGTH(N'dbo.service_location_types', N'department_scope') IS NOT NULL
                BEGIN
                    UPDATE [dbo].[service_location_types]
                    SET department_scope = N'All'
                    WHERE department_scope IS NULL;
                END
            END

            IF OBJECT_ID(N'[dbo].[service_locations]', N'U') IS NOT NULL
               AND COL_LENGTH(N'dbo.service_locations', N'company_id') IS NOT NULL
            BEGIN
                UPDATE sl
                SET sl.company_id = c.company_id
                FROM [dbo].[service_locations] AS sl
                CROSS APPLY (
                    SELECT TOP (1) company_id
                    FROM [dbo].[companies]
                    ORDER BY company_id
                ) AS c
                WHERE sl.company_id IS NULL;

                IF COL_LENGTH(N'dbo.service_locations', N'company_scope') IS NOT NULL
                BEGIN
                    UPDATE [dbo].[service_locations]
                    SET company_scope = N'Specific'
                    WHERE company_id IS NOT NULL;

                    UPDATE [dbo].[service_locations]
                    SET company_scope = N'All'
                    WHERE company_id IS NULL;
                END

                IF COL_LENGTH(N'dbo.service_locations', N'branch_scope') IS NOT NULL
                BEGIN
                    UPDATE [dbo].[service_locations]
                    SET branch_scope = N'All'
                    WHERE branch_scope IS NULL;
                END

                IF COL_LENGTH(N'dbo.service_locations', N'department_scope') IS NOT NULL
                BEGIN
                    UPDATE [dbo].[service_locations]
                    SET department_scope = N'All'
                    WHERE department_scope IS NULL;
                END
            END

            IF OBJECT_ID(N'[dbo].[service_routes]', N'U') IS NOT NULL
               AND COL_LENGTH(N'dbo.service_routes', N'company_id') IS NOT NULL
            BEGIN
                UPDATE sr
                SET sr.company_id = COALESCE(dep.company_id, arr.company_id, c.company_id),
                    sr.branch_id = COALESCE(dep.branch_id, arr.branch_id, sr.branch_id)
                FROM [dbo].[service_routes] AS sr
                LEFT JOIN [dbo].[service_locations] AS dep
                    ON dep.service_location_id = sr.departure_location_id
                LEFT JOIN [dbo].[service_locations] AS arr
                    ON arr.service_location_id = sr.arrival_location_id
                CROSS APPLY (
                    SELECT TOP (1) company_id
                    FROM [dbo].[companies]
                    ORDER BY company_id
                ) AS c
                WHERE sr.company_id IS NULL;

                IF COL_LENGTH(N'dbo.service_routes', N'company_scope') IS NOT NULL
                BEGIN
                    UPDATE [dbo].[service_routes]
                    SET company_scope = N'Specific'
                    WHERE company_id IS NOT NULL;

                    UPDATE [dbo].[service_routes]
                    SET company_scope = N'All'
                    WHERE company_id IS NULL;
                END

                IF COL_LENGTH(N'dbo.service_routes', N'branch_scope') IS NOT NULL
                BEGIN
                    UPDATE [dbo].[service_routes]
                    SET branch_scope = N'All'
                    WHERE branch_scope IS NULL;
                END

                IF COL_LENGTH(N'dbo.service_routes', N'department_scope') IS NOT NULL
                BEGIN
                    UPDATE [dbo].[service_routes]
                    SET department_scope = N'All'
                    WHERE department_scope IS NULL;
                END
            END
            """,
            transaction: transaction,
            commandTimeoutSeconds: timeout,
            cancellationToken: cancellationToken);
    }
}
