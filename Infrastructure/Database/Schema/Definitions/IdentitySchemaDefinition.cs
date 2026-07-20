using static Infrastructure.Database.Schema.SchemaTableBuilder;

namespace Infrastructure.Database.Schema.Definitions;

public sealed class IdentitySchemaDefinition : ISchemaDefinition
{
    public string FeatureName => "Identity";

    public IReadOnlyList<TableDefinition> Tables { get; } =
    [
        Table(
            "roles",
            [
                Col("role_id", "INT", identity: true, primaryKey: true),
                Col("role_name", "NVARCHAR(64)"),
                Col("description", "NVARCHAR(256)", nullable: true),
                Col("is_active", "BIT", defaultExpression: "1")
            ],
            indexes:
            [
                Idx("UX_roles_role_name", unique: true, "role_name")
            ]),

        Table(
            "departments",
            [
                Col("department_id", "INT", identity: true, primaryKey: true),
                Col("branch_id", "INT", nullable: true),
                Col("parent_department_id", "INT", nullable: true),
                Col("department_code", "NVARCHAR(64)", nullable: true),
                Col("department_name", "NVARCHAR(256)"),
                Col("description", "NVARCHAR(512)", nullable: true),
                Col("is_active", "BIT", defaultExpression: "1"),
                Col("created_at", "DATETIME2", defaultExpression: "SYSUTCDATETIME()"),
                Col("updated_at", "DATETIME2", defaultExpression: "SYSUTCDATETIME()")
            ],
            indexes:
            [
                Idx("IX_departments_parent_department_id", unique: false, "parent_department_id"),
                Idx("IX_departments_branch_id", unique: false, "branch_id"),
                // Per-company department codes via branch; filtered when both set.
                IdxFiltered(
                    "UX_departments_branch_id_department_code",
                    unique: true,
                    "[branch_id] IS NOT NULL AND [department_code] IS NOT NULL",
                    "branch_id",
                    "department_code")
            ],
            foreignKeys:
            [
                Fk("FK_departments_branches", "branch_id", "branches", "branch_id"),
                Fk("FK_departments_parent", "parent_department_id", "departments", "department_id")
            ]),

        Table(
            "branches",
            [
                Col("branch_id", "INT", identity: true, primaryKey: true),
                Col("company_id", "INT", nullable: true),
                Col("branch_code", "NVARCHAR(64)"),
                Col("branch_name", "NVARCHAR(256)"),
                Col("description", "NVARCHAR(512)", nullable: true),
                Col("address", "NVARCHAR(512)", nullable: true),
                Col("latitude", "DECIMAL(9,6)", nullable: true),
                Col("longitude", "DECIMAL(9,6)", nullable: true),
                Col("is_active", "BIT", defaultExpression: "1"),
                Col("created_at", "DATETIME2", defaultExpression: "SYSUTCDATETIME()"),
                Col("updated_at", "DATETIME2", defaultExpression: "SYSUTCDATETIME()")
            ],
            indexes:
            [
                // Per-company unique branch codes (legacy global UX_branches_branch_code dropped by SafeApply).
                IdxFiltered(
                    "UX_branches_company_id_branch_code",
                    unique: true,
                    "[company_id] IS NOT NULL",
                    "company_id",
                    "branch_code"),
                Idx("IX_branches_company_id", unique: false, "company_id")
            ],
            foreignKeys:
            [
                Fk("FK_branches_companies", "company_id", "companies", "company_id")
            ]),

        Table(
            "users",
            [
                Col("user_id", "INT", identity: true, primaryKey: true),
                Col("azure_object_id", "NVARCHAR(128)"),
                // LocalJwt login: username/password_hash (dev profile login root cause #1).
                Col("username", "NVARCHAR(128)", nullable: true),
                Col("password_hash", "NVARCHAR(512)", nullable: true),
                Col("password_changed_at", "DATETIME2", nullable: true),
                Col("failed_login_count", "INT", defaultExpression: "0"),
                Col("lockout_end", "DATETIME2", nullable: true),
                Col("email", "NVARCHAR(256)"),
                Col("full_name", "NVARCHAR(256)"),
                Col("title", "NVARCHAR(256)", nullable: true),
                Col("department_id", "INT", nullable: true),
                Col("branch_id", "INT", nullable: true),
                Col("role_id", "INT"),
                Col("birth_date", "DATE", nullable: true),
                Col("birth_month", "INT", nullable: true),
                Col("birth_day", "INT", nullable: true),
                Col("hire_date", "DATE", nullable: true),
                Col("phone", "NVARCHAR(64)", nullable: true),
                Col("photo_url", "NVARCHAR(1024)", nullable: true),
                Col("is_active", "BIT", defaultExpression: "1"),
                Col("last_login_at", "DATETIME2", nullable: true),
                Col("created_at", "DATETIME2", defaultExpression: "SYSUTCDATETIME()"),
                Col("updated_at", "DATETIME2", defaultExpression: "SYSUTCDATETIME()")
            ],
            indexes:
            [
                Idx("UX_users_azure_object_id", unique: true, "azure_object_id"),
                Idx("UX_users_email", unique: true, "email"),
                // Multiple NULL usernames exist for legacy rows — filtered unique index.
                IdxFiltered(
                    "UX_users_username",
                    unique: true,
                    "[username] IS NOT NULL",
                    "username"),
                Idx("IX_users_role_id", unique: false, "role_id"),
                Idx("IX_users_department_id", unique: false, "department_id"),
                Idx("IX_users_branch_id", unique: false, "branch_id")
            ],
            foreignKeys:
            [
                Fk("FK_users_roles", "role_id", "roles", "role_id"),
                Fk("FK_users_departments", "department_id", "departments", "department_id"),
                Fk("FK_users_branches", "branch_id", "branches", "branch_id")
            ]),

        Table(
            "external_user_identities",
            [
                Col("external_identity_id", "BIGINT", identity: true, primaryKey: true),
                Col("user_id", "INT"),
                Col("provider", "NVARCHAR(64)"),
                Col("external_subject", "NVARCHAR(256)"),
                Col("email", "NVARCHAR(256)", nullable: true),
                Col("created_at", "DATETIME2", defaultExpression: "SYSUTCDATETIME()")
            ],
            indexes:
            [
                Idx("UX_external_user_identities_provider_subject", unique: true, "provider", "external_subject"),
                Idx("IX_external_user_identities_user_id", unique: false, "user_id")
            ],
            foreignKeys:
            [
                Fk("FK_external_user_identities_users", "user_id", "users", "user_id", onDelete: "CASCADE")
            ])
    ];
}
