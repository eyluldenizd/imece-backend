using static Infrastructure.Database.Schema.SchemaTableBuilder;

namespace Infrastructure.Database.Schema.Definitions;

public sealed class AuditSchemaDefinition : ISchemaDefinition
{
    public string FeatureName => "Audit";

    public IReadOnlyList<TableDefinition> Tables { get; } =
    [
        Table(
            "audit_log",
            [
                Col("audit_id", "BIGINT", identity: true, primaryKey: true),
                Col("occurred_at", "DATETIME2", defaultExpression: "SYSUTCDATETIME()"),
                Col("action", "NVARCHAR(128)"),
                Col("entity_type", "NVARCHAR(128)", nullable: true),
                Col("entity_id", "NVARCHAR(128)", nullable: true),
                Col("user_id", "INT", nullable: true),
                Col("company_id", "INT", nullable: true),
                Col("trace_id", "NVARCHAR(128)", nullable: true),
                Col("client_ip", "NVARCHAR(64)", nullable: true),
                Col("user_agent", "NVARCHAR(512)", nullable: true),
                Col("client_application", "NVARCHAR(64)", nullable: true),
                Col("before_json", "NVARCHAR(MAX)", nullable: true),
                Col("after_json", "NVARCHAR(MAX)", nullable: true)
            ],
            indexes:
            [
                Idx("IX_audit_log_occurred_at", unique: false, "occurred_at"),
                Idx("IX_audit_log_entity", unique: false, "entity_type", "entity_id"),
                Idx("IX_audit_log_user_id", unique: false, "user_id")
            ])
    ];
}
