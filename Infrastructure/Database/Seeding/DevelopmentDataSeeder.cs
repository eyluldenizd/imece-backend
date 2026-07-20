using Infrastructure.Database.Connections;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Database.Seeding;

public sealed class DevelopmentDataSeeder : IDevelopmentDataSeeder
{
    private readonly IDbExecutor _executor;
    private readonly ILogger<DevelopmentDataSeeder> _logger;

    public DevelopmentDataSeeder(IDbExecutor executor, ILogger<DevelopmentDataSeeder> logger)
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
        // DevelopmentAuthenticationDefaults ile hizalı şirketler (1001/2002)
        // önce gelmeli; media_* FK'ları companies satırına bağımlıdır.
        await _executor.ExecuteNonQueryAsync(
            connection,
            """
            IF NOT EXISTS (SELECT 1 FROM [dbo].[companies] WHERE company_id = 1001)
            BEGIN
                SET IDENTITY_INSERT [dbo].[companies] ON;
                INSERT INTO [dbo].[companies] (company_id, company_code, company_name, is_active, created_at)
                VALUES (1001, N'COMPANY_A', N'Company A', 1, SYSUTCDATETIME());
                SET IDENTITY_INSERT [dbo].[companies] OFF;
            END

            IF NOT EXISTS (SELECT 1 FROM [dbo].[companies] WHERE company_id = 2002)
            BEGIN
                SET IDENTITY_INSERT [dbo].[companies] ON;
                INSERT INTO [dbo].[companies] (company_id, company_code, company_name, is_active, created_at)
                VALUES (2002, N'COMPANY_B', N'Company B', 1, SYSUTCDATETIME());
                SET IDENTITY_INSERT [dbo].[companies] OFF;
            END

            IF NOT EXISTS (SELECT 1 FROM [dbo].[branches] WHERE branch_code = N'HQ')
            BEGIN
                INSERT INTO [dbo].[branches] (branch_code, branch_name, is_active, created_at)
                VALUES (N'HQ', N'Headquarters', 1, SYSUTCDATETIME());
            END

            IF NOT EXISTS (SELECT 1 FROM [dbo].[departments] WHERE department_code = N'IT')
            BEGIN
                INSERT INTO [dbo].[departments] (department_code, department_name, is_active)
                VALUES (N'IT', N'Information Technology', 1);
            END

            IF NOT EXISTS (SELECT 1 FROM [dbo].[users] WHERE email = N'dev.admin@imece.local')
            BEGIN
                DECLARE @RoleId INT = (SELECT TOP 1 role_id FROM [dbo].[roles] WHERE role_name = N'global_admin');
                DECLARE @DeptId INT = (SELECT TOP 1 department_id FROM [dbo].[departments] WHERE department_code = N'IT');
                DECLARE @BranchId INT = (SELECT TOP 1 branch_id FROM [dbo].[branches] WHERE branch_code = N'HQ');
                DECLARE @CompanyId INT = (SELECT TOP 1 company_id FROM [dbo].[companies] WHERE company_code = N'DEFAULT');

                IF @RoleId IS NOT NULL
                BEGIN
                    INSERT INTO [dbo].[users]
                    (
                        azure_object_id, email, full_name, title,
                        department_id, branch_id, role_id,
                        is_active, created_at, updated_at
                    )
                    VALUES
                    (
                        N'dev-admin-object-id', N'dev.admin@imece.local', N'Dev Admin', N'Developer',
                        @DeptId, @BranchId, @RoleId,
                        1, SYSUTCDATETIME(), SYSUTCDATETIME()
                    );

                    DECLARE @UserId INT = SCOPE_IDENTITY();

                    IF @CompanyId IS NOT NULL AND @UserId IS NOT NULL
                    BEGIN
                        INSERT INTO [dbo].[user_company_roles] (user_id, company_id, role_id, is_active, created_at)
                        VALUES (@UserId, @CompanyId, @RoleId, 1, SYSUTCDATETIME());
                    END
                END
            END
            """,
            transaction: transaction,
            commandTimeoutSeconds: commandTimeoutSeconds,
            cancellationToken: cancellationToken);

        _logger.LogInformation("Development seed verileri uygulandı.");
    }
}
