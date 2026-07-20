namespace Infrastructure.Repositories.Queries;

internal static class UserCredentialQueries
{
    internal const string FindByUsername = """
        SELECT
            u.user_id           AS UserId,
            u.username          AS Username,
            u.password_hash     AS PasswordHash,
            u.azure_object_id   AS AzureObjectId,
            u.is_active         AS IsActive,
            u.failed_login_count AS FailedLoginCount,
            u.lockout_end       AS LockoutEnd
        FROM users u
        WHERE LOWER(LTRIM(RTRIM(u.username))) = @NormalizedUsername;
        """;

    internal const string UpdateLoginSuccess = """
        UPDATE users
        SET
            last_login_at = SYSUTCDATETIME(),
            failed_login_count = 0,
            lockout_end = NULL,
            updated_at = SYSUTCDATETIME()
        WHERE user_id = @UserId;
        """;

    internal const string UpdateLoginFailure = """
        UPDATE users
        SET
            failed_login_count = failed_login_count + 1,
            lockout_end = CASE
                WHEN failed_login_count + 1 >= 5
                THEN DATEADD(MINUTE, 15, SYSUTCDATETIME())
                ELSE lockout_end
            END,
            updated_at = SYSUTCDATETIME()
        WHERE user_id = @UserId;
        """;
}
