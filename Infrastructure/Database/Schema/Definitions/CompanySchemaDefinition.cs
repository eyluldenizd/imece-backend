using static Infrastructure.Database.Schema.SchemaTableBuilder;

namespace Infrastructure.Database.Schema.Definitions;

public sealed class CompanySchemaDefinition : ISchemaDefinition
{
    public string FeatureName => "Company";

    public IReadOnlyList<TableDefinition> Tables { get; } =
    [
        Table(
            "companies",
            [
                Col("company_id", "INT", identity: true, primaryKey: true),
                Col("company_code", "NVARCHAR(64)"),
                Col("company_name", "NVARCHAR(256)"),
                Col("description", "NVARCHAR(1024)", nullable: true),
                Col("logo_url", "NVARCHAR(1024)", nullable: true),
                Col("is_active", "BIT", defaultExpression: "1"),
                Col("created_at", "DATETIME2", defaultExpression: "SYSUTCDATETIME()"),
                Col("updated_at", "DATETIME2", defaultExpression: "SYSUTCDATETIME()")
            ],
            indexes:
            [
                Idx("UX_companies_company_code", unique: true, "company_code")
            ])
    ];
}
