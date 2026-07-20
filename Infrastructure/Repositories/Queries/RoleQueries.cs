namespace Infrastructure.Repositories.Queries;

public static class RoleQueries
{
    private const string SelectColumns = """
        SELECT
            role_id,
            role_name,
            description,
            is_active
        FROM roles
        """;

    public const string GetAll = $"""
        {SelectColumns}
        ORDER BY role_name ASC;
        """;

    public const string GetById = $"""
        {SelectColumns}
        WHERE role_id = @RoleId;
        """;

    public const string ExistsByName = """
        SELECT COUNT(1)
        FROM roles
        WHERE role_name = @RoleName
          AND (@ExcludeRoleId IS NULL OR role_id <> @ExcludeRoleId);
        """;

    public const string GetPermissionCodesByRoleId = """
        SELECT p.permission_code AS PermissionCode
        FROM role_permissions AS rp
        INNER JOIN permissions AS p ON p.permission_id = rp.permission_id
        WHERE rp.role_id = @RoleId
        ORDER BY p.permission_code ASC;
        """;

    public const string Create = """
        INSERT INTO roles
        (
            role_name,
            description,
            is_active
        )
        OUTPUT INSERTED.role_id
        VALUES
        (
            @RoleName,
            @Description,
            @IsActive
        );
        """;

    public const string Update = """
        UPDATE roles
        SET
            role_name = @RoleName,
            description = @Description,
            is_active = @IsActive
        WHERE role_id = @RoleId;
        """;

    public const string SoftDelete = """
        UPDATE roles
        SET is_active = 0
        WHERE role_id = @RoleId
          AND is_active = 1;
        """;

    public const string DeleteRolePermissions = """
        DELETE FROM role_permissions
        WHERE role_id = @RoleId;
        """;

    public const string InsertRolePermission = """
        INSERT INTO role_permissions (role_id, permission_id)
        VALUES (@RoleId, @PermissionId);
        """;
}
