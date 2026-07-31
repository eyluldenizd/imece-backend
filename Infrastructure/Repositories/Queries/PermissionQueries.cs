namespace Infrastructure.Repositories.Queries;

public static class PermissionQueries
{
    // PermissionRecord has no DbColumn — aliases must match property names.
    private const string SelectColumns = """
        SELECT
            permission_id AS PermissionId,
            permission_code AS PermissionCode,
            description AS Description
        FROM permissions
        """;

    public const string GetAll = $"""
        {SelectColumns}
        ORDER BY permission_code ASC;
        """;

    public const string GetById = $"""
        {SelectColumns}
        WHERE permission_id = @PermissionId;
        """;

    public const string ExistsByCode = """
        SELECT COUNT(1)
        FROM permissions
        WHERE permission_code = @PermissionCode
          AND (@ExcludePermissionId IS NULL OR permission_id <> @ExcludePermissionId);
        """;

    public const string GetAssignedRoleCount = """
        SELECT COUNT(1)
        FROM role_permissions
        WHERE permission_id = @PermissionId;
        """;

    public const string GetAssignedRoleCounts = """
        SELECT
            permission_id AS PermissionId,
            COUNT(1) AS RoleCount
        FROM role_permissions
        GROUP BY permission_id;
        """;

    public const string Create = """
        INSERT INTO permissions
        (
            permission_code,
            description
        )
        OUTPUT INSERTED.permission_id
        VALUES
        (
            @PermissionCode,
            @Description
        );
        """;

    public const string Update = """
        UPDATE permissions
        SET
            permission_code = @PermissionCode,
            description = @Description
        WHERE permission_id = @PermissionId;
        """;

    public const string Delete = """
        DELETE FROM permissions
        WHERE permission_id = @PermissionId;
        """;

    public const string GetByIdsTemplate = """
        SELECT permission_id AS PermissionId
        FROM permissions
        WHERE permission_id IN ({0});
        """;
}
