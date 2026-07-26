namespace Infrastructure.Repositories.Queries;

public static class UserQueries
{
    private const string EnrichedFrom = """
        FROM users AS u
        LEFT JOIN branches AS b ON b.branch_id = u.branch_id
        LEFT JOIN departments AS d ON d.department_id = u.department_id
        LEFT JOIN roles AS r ON r.role_id = u.role_id
        OUTER APPLY (
            SELECT TOP 1 ucr.company_id
            FROM user_company_roles AS ucr
            WHERE ucr.user_id = u.user_id
              AND ucr.is_active = 1
            ORDER BY ucr.user_company_role_id
        ) AS primary_ucr
        LEFT JOIN companies AS c ON c.company_id = COALESCE(primary_ucr.company_id, b.company_id)

        """;

    private const string EnrichedSelectColumns = """
        SELECT
            u.user_id,
            u.azure_object_id,
            u.username,
            u.email,
            u.full_name,
            u.title,
            COALESCE(primary_ucr.company_id, b.company_id) AS company_id,
            c.company_name,
            u.department_id,
            d.department_name,
            u.branch_id,
            b.branch_name,
            u.role_id,
            r.role_name,
            u.birth_date,
            u.hire_date,
            u.phone,
            u.photo_url,
            u.is_active,
            u.last_login_at,
            u.created_at,
            u.updated_at

        """;

    private const string SelectColumns = """
        SELECT
            user_id,
            azure_object_id,
            email,
            full_name,
            title,
            department_id,
            branch_id,
            role_id,
            birth_date,
            birth_month,
            birth_day,
            hire_date,
            phone,
            photo_url,
            is_active,
            last_login_at,
            created_at,
            updated_at
        FROM users

        """;

    public static readonly string GetAllEnriched =
        EnrichedSelectColumns +
        EnrichedFrom +
        """
        WHERE
        """ +
        CompanyScopeSql.UserMembershipFilter +
        """
        ORDER BY u.full_name ASC;
        """;

    public static readonly string GetActiveEnriched =
        EnrichedSelectColumns +
        EnrichedFrom +
        """
        WHERE u.is_active = 1
          AND
        """ +
        CompanyScopeSql.UserMembershipFilter +
        """
        ORDER BY u.full_name ASC;
        """;

    public static readonly string GetByIdEnriched =
        EnrichedSelectColumns +
        EnrichedFrom +
        """
        WHERE u.user_id = @UserId;
        """;

    public static readonly string SearchEnriched =
        EnrichedSelectColumns +
        EnrichedFrom +
        """
        WHERE
            (
                u.full_name LIKE @SearchText
                OR u.email LIKE @SearchText
                OR u.title LIKE @SearchText
            )
          AND
        """ +
        CompanyScopeSql.UserMembershipFilter +
        """
        ORDER BY u.full_name ASC;
        """;

    public static readonly string GetAll =
        SelectColumns +
        """
        ORDER BY full_name ASC;
        """;

    public static readonly string GetActive =
        SelectColumns +
        """
        WHERE is_active = 1
        ORDER BY full_name ASC;
        """;

    public static readonly string GetById =
        SelectColumns +
        """
        WHERE user_id = @UserId;
        """;

    public static readonly string GetByAzureObjectId =
        SelectColumns +
        """
        WHERE azure_object_id = @AzureObjectId;
        """;

    public static readonly string GetByEmail =
        SelectColumns +
        """
        WHERE email = @Email;
        """;

    public static readonly string Search =
        SelectColumns +
        """
        WHERE
            full_name LIKE @SearchText
            OR email LIKE @SearchText
            OR title LIKE @SearchText
        ORDER BY full_name ASC;
        """;

    public const string Create = """
        INSERT INTO users
        (
            azure_object_id,
            username,
            password_hash,
            password_changed_at,
            email,
            full_name,
            title,
            department_id,
            branch_id,
            role_id,
            birth_date,
            birth_month,
            birth_day,
            hire_date,
            phone,
            photo_url,
            is_active,
            created_at,
            updated_at
        )
        VALUES
        (
            @AzureObjectId,
            @Username,
            @PasswordHash,
            @PasswordChangedAt,
            @Email,
            @FullName,
            @Title,
            @DepartmentId,
            @BranchId,
            @RoleId,
            @BirthDate,
            @BirthMonth,
            @BirthDay,
            @HireDate,
            @Phone,
            @PhotoUrl,
            @IsActive,
            GETUTCDATE(),
            GETUTCDATE()
        );

        SELECT CAST(SCOPE_IDENTITY() AS INT);
        """;

    public const string Update = """
        UPDATE users
        SET
            full_name = @FullName,
            title = @Title,
            department_id = @DepartmentId,
            branch_id = @BranchId,
            role_id = @RoleId,
            birth_date = @BirthDate,
            birth_month = @BirthMonth,
            birth_day = @BirthDay,
            hire_date = @HireDate,
            phone = @Phone,
            photo_url = @PhotoUrl,
            is_active = @IsActive,
            updated_at = SYSDATETIME()
        WHERE user_id = @UserId;
        """;

    public const string UpdateLastLogin = """
        UPDATE users
        SET
            last_login_at = SYSDATETIME(),
            updated_at = SYSDATETIME()
        WHERE user_id = @UserId;
        """;

    public const string ExistsByUsername = """
        SELECT COUNT(1)
        FROM users
        WHERE username = @Username
          AND (@ExcludeUserId IS NULL OR user_id <> @ExcludeUserId);
        """;

    public const string UpdatePassword = """
        UPDATE users
        SET
            password_hash = @PasswordHash,
            password_changed_at = @PasswordChangedAt,
            updated_at = SYSDATETIME()
        WHERE user_id = @UserId;
        """;

    public static readonly string GetActiveLookup =
        """
        SELECT
            u.user_id,
            u.full_name,
            u.email
        FROM users AS u
        WHERE u.is_active = 1
          AND
        """ +
        CompanyScopeSql.UserMembershipFilter +
        """
        ORDER BY u.full_name ASC;
        """;
}
