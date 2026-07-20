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

    public const string GetByIdsTemplate = """
        SELECT permission_id AS PermissionId
        FROM permissions
        WHERE permission_id IN ({0});
        """;
}
