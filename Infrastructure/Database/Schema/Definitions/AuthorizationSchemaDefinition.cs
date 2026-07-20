using static Infrastructure.Database.Schema.SchemaTableBuilder;

namespace Infrastructure.Database.Schema.Definitions;

public sealed class AuthorizationSchemaDefinition : ISchemaDefinition
{
    public string FeatureName => "Authorization";

    public IReadOnlyList<TableDefinition> Tables { get; } =
    [
        Table(
            "permissions",
            [
                Col("permission_id", "INT", identity: true, primaryKey: true),
                Col("permission_code", "NVARCHAR(128)"),
                Col("description", "NVARCHAR(256)", nullable: true)
            ],
            indexes:
            [
                Idx("UX_permissions_permission_code", unique: true, "permission_code")
            ]),

        Table(
            "role_permissions",
            [
                Col("role_id", "INT", primaryKey: true),
                Col("permission_id", "INT", primaryKey: true)
            ],
            foreignKeys:
            [
                Fk("FK_role_permissions_roles", "role_id", "roles", "role_id", onDelete: "CASCADE"),
                Fk("FK_role_permissions_permissions", "permission_id", "permissions", "permission_id", onDelete: "CASCADE")
            ]),

        Table(
            "user_company_roles",
            [
                Col("user_company_role_id", "BIGINT", identity: true, primaryKey: true),
                Col("user_id", "INT"),
                Col("company_id", "INT"),
                Col("role_id", "INT"),
                Col("is_active", "BIT", defaultExpression: "1"),
                Col("created_at", "DATETIME2", defaultExpression: "SYSUTCDATETIME()")
            ],
            indexes:
            [
                Idx("UX_user_company_roles_user_company_role", unique: true, "user_id", "company_id", "role_id"),
                Idx("IX_user_company_roles_company_id", unique: false, "company_id")
            ],
            foreignKeys:
            [
                Fk("FK_user_company_roles_users", "user_id", "users", "user_id", onDelete: "CASCADE"),
                Fk("FK_user_company_roles_companies", "company_id", "companies", "company_id", onDelete: "CASCADE"),
                Fk("FK_user_company_roles_roles", "role_id", "roles", "role_id")
            ])
    ];
}
