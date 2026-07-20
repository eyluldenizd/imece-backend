using static Infrastructure.Database.Schema.SchemaTableBuilder;

namespace Infrastructure.Database.Schema.Definitions;

public sealed class MultiCompanyTargetedContentSchemaDefinition : ISchemaDefinition
{
    public string FeatureName => "MultiCompanyTargetedContent";

    public IReadOnlyList<TableDefinition> Tables { get; } =
    [
        Table(
            "content_company_targets",
            [
                Col("content_company_target_id", "BIGINT", identity: true, primaryKey: true),
                Col("content_type", "NVARCHAR(64)"),
                Col("content_id", "BIGINT"),
                Col("company_id", "INT"),
                Col("created_at", "DATETIME2", defaultExpression: "SYSUTCDATETIME()")
            ],
            indexes:
            [
                Idx("UX_content_company_targets_type_id_company", unique: true, "content_type", "content_id", "company_id"),
                Idx("IX_content_company_targets_company_id", unique: false, "company_id")
            ],
            foreignKeys:
            [
                Fk("FK_content_company_targets_companies", "company_id", "companies", "company_id", onDelete: "CASCADE")
            ])
    ];
}
