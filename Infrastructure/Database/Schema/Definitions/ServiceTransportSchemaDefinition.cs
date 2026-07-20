using static Infrastructure.Database.Schema.SchemaTableBuilder;

namespace Infrastructure.Database.Schema.Definitions;

public sealed class ServiceTransportSchemaDefinition : ISchemaDefinition
{
    public string FeatureName => "ServiceTransport";

    public IReadOnlyList<TableDefinition> Tables { get; } =
    [
        Table(
            "service_location_types",
            [
                Col("service_location_type_id", "INT", identity: true, primaryKey: true),
                Col("name", "NVARCHAR(64)"),
                Col("description", "NVARCHAR(512)", nullable: true),
                Col("icon_url", "NVARCHAR(1024)", nullable: true),
                Col("color_key", "NVARCHAR(32)", nullable: true),
                Col("sort_order", "INT", defaultExpression: "0"),
                Col("is_active", "BIT", defaultExpression: "1"),
                Col("created_at", "DATETIME2", defaultExpression: "SYSUTCDATETIME()"),
                Col("updated_at", "DATETIME2", defaultExpression: "SYSUTCDATETIME()")
            ],
            indexes:
            [
                Idx("UX_service_location_types_name", unique: true, "name"),
                Idx("IX_service_location_types_sort_order", unique: false, "sort_order")
            ]),

        Table(
            "service_locations",
            [
                Col("service_location_id", "BIGINT", identity: true, primaryKey: true),
                Col("company_id", "INT", nullable: true),
                Col("branch_id", "INT", nullable: true),
                Col("name", "NVARCHAR(256)"),
                Col("service_location_type_id", "INT", nullable: true),
                Col("location_type", "NVARCHAR(32)"),
                Col("address", "NVARCHAR(512)", nullable: true),
                Col("latitude", "DECIMAL(9,6)", nullable: true),
                Col("longitude", "DECIMAL(9,6)", nullable: true),
                Col("is_active", "BIT", defaultExpression: "1"),
                Col("created_at", "DATETIME2", defaultExpression: "SYSUTCDATETIME()"),
                Col("updated_at", "DATETIME2", defaultExpression: "SYSUTCDATETIME()")
            ],
            indexes:
            [
                Idx("IX_service_locations_company_id", unique: false, "company_id"),
                Idx("IX_service_locations_branch_id", unique: false, "branch_id"),
                Idx("IX_service_locations_type_id", unique: false, "service_location_type_id")
            ],
            foreignKeys:
            [
                Fk("FK_service_locations_companies", "company_id", "companies", "company_id"),
                Fk("FK_service_locations_branches", "branch_id", "branches", "branch_id"),
                Fk("FK_service_locations_types", "service_location_type_id", "service_location_types", "service_location_type_id")
            ]),

        Table(
            "service_route_stops",
            [
                Col("service_route_stop_id", "BIGINT", identity: true, primaryKey: true),
                Col("service_route_id", "BIGINT"),
                Col("service_location_id", "BIGINT"),
                Col("stop_order", "INT"),
                Col("arrival_time", "TIME", nullable: true),
                Col("departure_time", "TIME", nullable: true),
                Col("notes", "NVARCHAR(512)", nullable: true),
                Col("is_active", "BIT", defaultExpression: "1")
            ],
            indexes:
            [
                Idx("UX_service_route_stops_route_order", unique: true, "service_route_id", "stop_order"),
                Idx("IX_service_route_stops_route_id", unique: false, "service_route_id")
            ],
            foreignKeys:
            [
                Fk("FK_service_route_stops_routes", "service_route_id", "service_routes", "service_route_id"),
                Fk("FK_service_route_stops_locations", "service_location_id", "service_locations", "service_location_id")
            ])
    ];
}
