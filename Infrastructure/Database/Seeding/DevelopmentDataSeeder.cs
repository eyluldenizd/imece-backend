using Infrastructure.Database.Connections;
using Infrastructure.Database.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Database.Seeding;

public sealed class DevelopmentDataSeeder : IDevelopmentDataSeeder
{
    private readonly IDbExecutor _executor;
    private readonly IRealisticDevelopmentContentSeeder _contentSeeder;
    private readonly IOptions<DatabaseSchemaOptions> _schemaOptions;
    private readonly ILogger<DevelopmentDataSeeder> _logger;
    private readonly PasswordHasher<object> _passwordHasher = new();

    public DevelopmentDataSeeder(
        IDbExecutor executor,
        IRealisticDevelopmentContentSeeder contentSeeder,
        IOptions<DatabaseSchemaOptions> schemaOptions,
        ILogger<DevelopmentDataSeeder> logger)
    {
        _executor = executor;
        _contentSeeder = contentSeeder;
        _schemaOptions = schemaOptions;
        _logger = logger;
    }

    public async Task SeedAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        int commandTimeoutSeconds,
        CancellationToken cancellationToken = default)
    {
        await SeedCompaniesAsync(connection, transaction, commandTimeoutSeconds, cancellationToken);
        await SeedBranchesAsync(connection, transaction, commandTimeoutSeconds, cancellationToken);
        await SeedDepartmentsAsync(connection, transaction, commandTimeoutSeconds, cancellationToken);

        await SeedLocalJwtUserAsync(
            connection, transaction, commandTimeoutSeconds,
            username: "admin", password: "Admin123!", roleName: "global_admin",
            externalId: "local:admin", email: "admin@imece.local", fullName: "Sistem Yöneticisi",
            companyIds: [1001, 2002], cancellationToken);

        await SeedLocalJwtUserAsync(
            connection, transaction, commandTimeoutSeconds,
            username: "companyadmin", password: "CompanyAdmin123!", roleName: "company_admin",
            externalId: "local:companyadmin", email: "companyadmin@imece.local", fullName: "Şirket Yöneticisi",
            companyIds: [1001], cancellationToken);

        await SeedLocalJwtUserAsync(
            connection, transaction, commandTimeoutSeconds,
            username: "editor", password: "Editor123!", roleName: "editor",
            externalId: "local:editor", email: "editor@imece.local", fullName: "İçerik Editörü",
            companyIds: [1001], cancellationToken);

        await SeedLocalJwtUserAsync(
            connection, transaction, commandTimeoutSeconds,
            username: "viewer", password: "Viewer123!", roleName: "user",
            externalId: "local:viewer", email: "viewer@imece.local", fullName: "Standart Kullanıcı",
            companyIds: [1001], cancellationToken);

        await SeedAdditionalUsersAsync(connection, transaction, commandTimeoutSeconds, cancellationToken);

        var options = _schemaOptions.Value;
        if (options.SeedRealisticContent)
        {
            await _contentSeeder.SeedAsync(
                connection,
                transaction,
                commandTimeoutSeconds,
                options.DevelopmentSeedVersion,
                cancellationToken);
        }

        await NormalizeMediaCatalogAsync(connection, transaction, commandTimeoutSeconds, cancellationToken);

        _logger.LogInformation("Development seed verileri uygulandı.");
    }

    private async Task SeedCompaniesAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        int timeout,
        CancellationToken cancellationToken)
    {
        var companies = new (int Id, string Code, string Name)[]
        {
            (1001, "ATLAS", "Atlas Teknoloji A.Ş."),
            (2002, "NOVA", "Nova Enerji A.Ş."),
            (3003, "KARSA", "Karadeniz Holding A.Ş."),
            (4004, "EGEHOLD", "Ege Yatırım A.Ş.")
        };

        foreach (var (id, code, name) in companies)
        {
            await _executor.ExecuteNonQueryAsync(
                connection,
                """
                IF EXISTS (SELECT 1 FROM [dbo].[companies] WHERE company_id = @CompanyId)
                BEGIN
                    UPDATE [dbo].[companies]
                    SET company_code = @Code,
                        company_name = @Name,
                        is_active = 1,
                        updated_at = SYSUTCDATETIME()
                    WHERE company_id = @CompanyId;
                END
                ELSE
                BEGIN
                    SET IDENTITY_INSERT [dbo].[companies] ON;
                    INSERT INTO [dbo].[companies] (company_id, company_code, company_name, is_active, created_at, updated_at)
                    VALUES (@CompanyId, @Code, @Name, 1, SYSUTCDATETIME(), SYSUTCDATETIME());
                    SET IDENTITY_INSERT [dbo].[companies] OFF;
                END
                """,
                parameters:
                [
                    new SqlParameter("@CompanyId", id),
                    new SqlParameter("@Code", code),
                    new SqlParameter("@Name", name)
                ],
                transaction: transaction,
                commandTimeoutSeconds: timeout,
                cancellationToken: cancellationToken);
        }
    }

    private async Task SeedBranchesAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        int timeout,
        CancellationToken cancellationToken)
    {
        var branches = new (int CompanyId, string Code, string Name, bool Active)[]
        {
            (1001, "ATLAS-MERKEZ", "Atlas Genel Müdürlük", true),
            (1001, "ATLAS-ANK", "Ankara Bölge Müdürlüğü", true),
            (1001, "ATLAS-IZM", "İzmir Bölge Müdürlüğü", true),
            (1001, "ATLAS-BUR", "Bursa Bölge Müdürlüğü", true),
            (1001, "ATLAS-IST", "İstanbul Satış Ofisi", true),
            (2002, "NOVA-MERKEZ", "Nova Genel Müdürlük", true),
            (2002, "NOVA-ADN", "Adana Enerji Tesisi", true),
            (2002, "NOVA-ANT", "Antalya Güneş Santrali", true),
            (2002, "NOVA-GAZ", "Gaziantep Rüzgar Santrali", false),
            (3003, "KARSA-MERKEZ", "Karadeniz Holding Merkez", true),
            (3003, "KARSA-TRB", "Trabzon Liman İşletmesi", true),
            (4004, "EGE-MERKEZ", "Ege Yatırım Merkez", true),
            (4004, "EGE-AYD", "Aydın Tarım Kooperatifi", true)
        };

        foreach (var (companyId, code, name, active) in branches)
        {
            await _executor.ExecuteNonQueryAsync(
                connection,
                """
                IF NOT EXISTS (SELECT 1 FROM [dbo].[branches] WHERE branch_code = @Code AND company_id = @CompanyId)
                BEGIN
                    INSERT INTO [dbo].[branches]
                    (company_id, branch_code, branch_name, is_active, created_at, updated_at)
                    VALUES (@CompanyId, @Code, @Name, @IsActive, SYSUTCDATETIME(), SYSUTCDATETIME());
                END
                """,
                parameters:
                [
                    new SqlParameter("@CompanyId", companyId),
                    new SqlParameter("@Code", code),
                    new SqlParameter("@Name", name),
                    new SqlParameter("@IsActive", active)
                ],
                transaction: transaction,
                commandTimeoutSeconds: timeout,
                cancellationToken: cancellationToken);
        }
    }

    private async Task SeedDepartmentsAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        int timeout,
        CancellationToken cancellationToken)
    {
        await _executor.ExecuteNonQueryAsync(
            connection,
            """
            DECLARE @BranchId INT;
            DECLARE @CompanyId INT;
            DECLARE @BranchCode NVARCHAR(64);
            DECLARE @DeptCode NVARCHAR(64);
            DECLARE @DeptName NVARCHAR(256);

            DECLARE branch_cursor CURSOR LOCAL FAST_FORWARD FOR
                SELECT branch_id, company_id, branch_code
                FROM [dbo].[branches]
                ORDER BY branch_id;

            OPEN branch_cursor;
            FETCH NEXT FROM branch_cursor INTO @BranchId, @CompanyId, @BranchCode;

            WHILE @@FETCH_STATUS = 0
            BEGIN
                DECLARE @DeptIndex INT = 0;

                WHILE @DeptIndex < 6
                BEGIN
                    SET @DeptCode = @BranchCode + N'-' +
                        CASE @DeptIndex
                            WHEN 0 THEN N'IK'
                            WHEN 1 THEN N'BT'
                            WHEN 2 THEN N'FIN'
                            WHEN 3 THEN N'OPS'
                            WHEN 4 THEN N'SAT'
                            WHEN 5 THEN N'PAZ'
                        END;

                    SET @DeptName =
                        CASE @DeptIndex
                            WHEN 0 THEN N'İnsan Kaynakları'
                            WHEN 1 THEN N'Bilgi Teknolojileri'
                            WHEN 2 THEN N'Finans'
                            WHEN 3 THEN N'Operasyon'
                            WHEN 4 THEN N'Satış'
                            WHEN 5 THEN N'Pazarlama'
                        END;

                    IF NOT EXISTS (
                        SELECT 1 FROM [dbo].[departments]
                        WHERE department_code = @DeptCode AND branch_id = @BranchId)
                    BEGIN
                        INSERT INTO [dbo].[departments]
                        (branch_id, department_code, department_name, is_active, created_at, updated_at)
                        VALUES (@BranchId, @DeptCode, @DeptName, 1, SYSUTCDATETIME(), SYSUTCDATETIME());
                    END

                    SET @DeptIndex = @DeptIndex + 1;
                END

                IF @BranchCode LIKE N'%-MERKEZ'
                BEGIN
                    SET @DeptCode = @BranchCode + N'-HUK';
                    IF NOT EXISTS (
                        SELECT 1 FROM [dbo].[departments]
                        WHERE department_code = @DeptCode AND branch_id = @BranchId)
                    BEGIN
                        INSERT INTO [dbo].[departments]
                        (branch_id, department_code, department_name, is_active, created_at, updated_at)
                        VALUES (@BranchId, @DeptCode, N'Hukuk', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
                    END

                    SET @DeptCode = @BranchCode + N'-ARGE';
                    IF NOT EXISTS (
                        SELECT 1 FROM [dbo].[departments]
                        WHERE department_code = @DeptCode AND branch_id = @BranchId)
                    BEGIN
                        INSERT INTO [dbo].[departments]
                        (branch_id, department_code, department_name, is_active, created_at, updated_at)
                        VALUES (@BranchId, @DeptCode, N'Ar-Ge', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
                    END
                END

                FETCH NEXT FROM branch_cursor INTO @BranchId, @CompanyId, @BranchCode;
            END

            CLOSE branch_cursor;
            DEALLOCATE branch_cursor;
            """,
            transaction: transaction,
            commandTimeoutSeconds: timeout,
            cancellationToken: cancellationToken);
    }

    private async Task SeedAdditionalUsersAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        int timeout,
        CancellationToken cancellationToken)
    {
        var firstNames = new[]
        {
            "Ahmet", "Ayşe", "Mehmet", "Fatma", "Mustafa", "Zeynep", "Ali", "Elif",
            "Hüseyin", "Merve", "Emre", "Selin", "Burak", "Deniz", "Can", "Esra",
            "Oğuz", "Gizem", "Serkan", "Pınar", "Tolga", "Ceren", "Kemal", "Büşra",
            "Murat", "Seda", "Barış", "Hande", "Volkan", "Aslı", "Yusuf", "Nazlı",
            "Erhan", "Derya", "Onur", "Melis", "Kaan", "Tuğba", "Sinan", "Ebru"
        };

        var lastNames = new[]
        {
            "Yılmaz", "Kaya", "Demir", "Çelik", "Şahin", "Yıldız", "Aydın", "Arslan",
            "Doğan", "Koç", "Kurt", "Öztürk", "Polat", "Erdoğan", "Aksoy", "Güneş",
            "Taş", "Bulut", "Korkmaz", "Aslan", "Tekin", "Uçar", "Bozkurt", "Karaca",
            "Sezer", "Tunç", "Bayrak", "Eren", "Sönmez", "Acar", "Yavuz", "Kaplan",
            "Duman", "Gencer", "Altun", "Işık", "Vural", "Ceylan", "Dağ", "Tan"
        };

        var roles = new[] { "user", "editor", "company_admin", "user", "user" };
        var companies = new[] { 1001, 1001, 1001, 2002, 3003, 4004 };

        for (var i = 0; i < firstNames.Length; i++)
        {
            var fullName = $"{firstNames[i]} {lastNames[i]}";
            var username = $"{firstNames[i].ToLowerInvariant()}.{lastNames[i].ToLowerInvariant()}";
            var email = $"{username}@atlas.com.tr";
            var role = roles[i % roles.Length];
            var companyId = companies[i % companies.Length];
            var isActive = i % 9 != 0;

            await SeedEmployeeUserAsync(
                connection,
                transaction,
                timeout,
                username,
                password: "Welcome123!",
                roleName: role,
                externalId: $"local:employee:{i + 1}",
                email,
                fullName,
                companyId,
                isActive,
                cancellationToken);
        }
    }

    private async Task SeedEmployeeUserAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        int commandTimeoutSeconds,
        string username,
        string password,
        string roleName,
        string externalId,
        string email,
        string fullName,
        int companyId,
        bool isActive,
        CancellationToken cancellationToken)
    {
        var passwordHash = _passwordHasher.HashPassword(null!, password);

        await _executor.ExecuteNonQueryAsync(
            connection,
            """
            IF NOT EXISTS (SELECT 1 FROM [dbo].[users] WHERE username = @Username)
            BEGIN
                DECLARE @RoleId INT = (SELECT TOP 1 role_id FROM [dbo].[roles] WHERE role_name = @RoleName);
                DECLARE @BranchId INT = (
                    SELECT TOP 1 branch_id FROM [dbo].[branches]
                    WHERE company_id = @CompanyId ORDER BY branch_id);
                DECLARE @DeptId INT = (
                    SELECT TOP 1 d.department_id
                    FROM [dbo].[departments] AS d
                    INNER JOIN [dbo].[branches] AS b ON b.branch_id = d.branch_id
                    WHERE b.company_id = @CompanyId
                    ORDER BY d.department_id);

                IF @RoleId IS NOT NULL AND @BranchId IS NOT NULL
                BEGIN
                    INSERT INTO [dbo].[users]
                    (
                        azure_object_id, username, password_hash, password_changed_at,
                        email, full_name, title,
                        department_id, branch_id, role_id,
                        is_active, created_at, updated_at
                    )
                    VALUES
                    (
                        @ExternalId, @Username, @PasswordHash, SYSUTCDATETIME(),
                        @Email, @FullName, N'Çalışan',
                        @DeptId, @BranchId, @RoleId,
                        @IsActive, SYSUTCDATETIME(), SYSUTCDATETIME()
                    );

                    DECLARE @UserId INT = SCOPE_IDENTITY();

                    IF @UserId IS NOT NULL
                    BEGIN
                        INSERT INTO [dbo].[user_company_roles] (user_id, company_id, role_id, is_active, created_at)
                        VALUES (@UserId, @CompanyId, @RoleId, @IsActive, SYSUTCDATETIME());
                    END
                END
            END
            """,
            parameters:
            [
                new SqlParameter("@Username", username),
                new SqlParameter("@PasswordHash", passwordHash),
                new SqlParameter("@RoleName", roleName),
                new SqlParameter("@ExternalId", externalId),
                new SqlParameter("@Email", email),
                new SqlParameter("@FullName", fullName),
                new SqlParameter("@CompanyId", companyId),
                new SqlParameter("@IsActive", isActive)
            ],
            transaction: transaction,
            commandTimeoutSeconds: commandTimeoutSeconds,
            cancellationToken: cancellationToken);
    }

    private async Task SeedLocalJwtUserAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        int commandTimeoutSeconds,
        string username,
        string password,
        string roleName,
        string externalId,
        string email,
        string fullName,
        int[] companyIds,
        CancellationToken cancellationToken)
    {
        var passwordHash = _passwordHasher.HashPassword(null!, password);

        await _executor.ExecuteNonQueryAsync(
            connection,
            """
            IF NOT EXISTS (SELECT 1 FROM [dbo].[users] WHERE username = @Username)
            BEGIN
                DECLARE @RoleId INT = (SELECT TOP 1 role_id FROM [dbo].[roles] WHERE role_name = @RoleName);
                DECLARE @DeptId INT = (
                    SELECT TOP 1 d.department_id
                    FROM [dbo].[departments] AS d
                    INNER JOIN [dbo].[branches] AS b ON b.branch_id = d.branch_id
                    WHERE b.company_id = 1001 AND d.department_code LIKE N'ATLAS-MERKEZ-BT%'
                    ORDER BY d.department_id);
                DECLARE @BranchId INT = (
                    SELECT TOP 1 branch_id FROM [dbo].[branches]
                    WHERE company_id = 1001 AND branch_code = N'ATLAS-MERKEZ');

                IF @RoleId IS NOT NULL
                BEGIN
                    INSERT INTO [dbo].[users]
                    (
                        azure_object_id, username, password_hash, password_changed_at,
                        email, full_name, title,
                        department_id, branch_id, role_id,
                        is_active, created_at, updated_at
                    )
                    VALUES
                    (
                        @ExternalId, @Username, @PasswordHash, SYSUTCDATETIME(),
                        @Email, @FullName, N'Yerel Geliştirme Kullanıcısı',
                        @DeptId, @BranchId, @RoleId,
                        1, SYSUTCDATETIME(), SYSUTCDATETIME()
                    );

                    DECLARE @UserId INT = SCOPE_IDENTITY();

                    IF @UserId IS NOT NULL
                    BEGIN
                        INSERT INTO [dbo].[user_company_roles] (user_id, company_id, role_id, is_active, created_at)
                        SELECT @UserId, c.company_id, @RoleId, 1, SYSUTCDATETIME()
                        FROM (SELECT CAST(value AS INT) AS company_id FROM OPENJSON(@CompanyIdsJson)) AS c;
                    END
                END
            END
            """,
            parameters:
            [
                new SqlParameter("@Username", username),
                new SqlParameter("@PasswordHash", passwordHash),
                new SqlParameter("@RoleName", roleName),
                new SqlParameter("@ExternalId", externalId),
                new SqlParameter("@Email", email),
                new SqlParameter("@FullName", fullName),
                new SqlParameter("@CompanyIdsJson", $"[{string.Join(",", companyIds)}]")
            ],
            transaction: transaction,
            commandTimeoutSeconds: commandTimeoutSeconds,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Admin galeri/doküman sayfaları media_type filtrelerine uyum için idempotent normalizasyon.
    /// </summary>
    private async Task NormalizeMediaCatalogAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        int timeout,
        CancellationToken cancellationToken)
    {
        await _executor.ExecuteNonQueryAsync(
            connection,
            """
            UPDATE [dbo].[media_files]
            SET media_type = N'Photo',
                content_type = N'image/png',
                file_extension = N'.png'
            WHERE media_file_id <= 20
              AND media_type <> N'Photo';

            UPDATE [dbo].[media_files]
            SET media_type = N'Document',
                content_type = N'application/pdf',
                file_extension = N'.pdf',
                original_file_name = REPLACE(original_file_name, N'.png', N'.pdf'),
                stored_file_name = REPLACE(stored_file_name, N'.png', N'.pdf'),
                relative_path = REPLACE(relative_path, N'.png', N'.pdf')
            WHERE media_file_id > 20
              AND media_file_id <= 35
              AND media_type <> N'Document';
            """,
            transaction: transaction,
            commandTimeoutSeconds: timeout,
            cancellationToken: cancellationToken);
    }
}
