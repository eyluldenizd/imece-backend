namespace Infrastructure.Repositories.Queries;

public static class UserQueries
{
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
            user_id,
            full_name,
            email
        FROM users
        WHERE is_active = 1
        ORDER BY full_name ASC;
        """;
}
