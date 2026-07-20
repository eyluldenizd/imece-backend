using static Infrastructure.Database.Schema.SchemaTableBuilder;

namespace Infrastructure.Database.Schema.Definitions;

public sealed class GlobalContentSchemaDefinition : ISchemaDefinition
{
    public string FeatureName => "GlobalContent";

    public IReadOnlyList<TableDefinition> Tables { get; } =
    [
        Table(
            "services",
            [
                Col("service_id", "BIGINT", identity: true, primaryKey: true),
                Col("name", "NVARCHAR(256)"),
                Col("description", "NVARCHAR(MAX)", nullable: true),
                Col("icon", "NVARCHAR(256)", nullable: true),
                Col("is_active", "BIT", defaultExpression: "1"),
                Col("created_at", "DATETIME2", defaultExpression: "SYSUTCDATETIME()"),
                Col("updated_at", "DATETIME2", defaultExpression: "SYSUTCDATETIME()")
            ]),

        Table(
            "corporate_apps",
            [
                Col("app_id", "BIGINT", identity: true, primaryKey: true),
                Col("title", "NVARCHAR(256)"),
                Col("description", "NVARCHAR(MAX)", nullable: true),
                Col("url", "NVARCHAR(1024)"),
                Col("category", "NVARCHAR(128)", nullable: true),
                Col("is_active", "BIT", defaultExpression: "1")
            ]),

        Table(
            "communication_channels",
            [
                Col("channel_id", "BIGINT", identity: true, primaryKey: true),
                Col("channel_name", "NVARCHAR(256)"),
                Col("type", "NVARCHAR(64)"),
                Col("address_url", "NVARCHAR(1024)"),
                Col("department_in_charge", "NVARCHAR(256)", nullable: true)
            ]),

        Table(
            "upcoming_events",
            [
                Col("event_id", "BIGINT", identity: true, primaryKey: true),
                Col("title", "NVARCHAR(512)"),
                Col("description", "NVARCHAR(MAX)", nullable: true),
                Col("event_date", "DATETIME2"),
                Col("location", "NVARCHAR(256)", nullable: true),
                Col("is_active", "BIT", defaultExpression: "1")
            ],
            indexes:
            [
                Idx("IX_upcoming_events_event_date", unique: false, "event_date")
            ]),

        Table(
            "campaigns",
            [
                Col("campaign_id", "BIGINT", identity: true, primaryKey: true),
                Col("title", "NVARCHAR(512)"),
                Col("description", "NVARCHAR(MAX)", nullable: true),
                Col("image_url", "NVARCHAR(1024)", nullable: true),
                Col("target_url", "NVARCHAR(1024)", nullable: true),
                Col("start_date", "DATETIME2"),
                Col("end_date", "DATETIME2"),
                Col("is_active", "BIT", defaultExpression: "1")
            ]),

        Table(
            "dishes",
            [
                Col("dish_id", "INT", identity: true, primaryKey: true),
                Col("dish_name", "NVARCHAR(256)"),
                Col("category", "NVARCHAR(128)"),
                Col("is_active", "BIT", defaultExpression: "1"),
                Col("created_at", "DATETIME2", defaultExpression: "SYSUTCDATETIME()")
            ]),

        Table(
            "weeks",
            [
                Col("week_id", "INT", identity: true, primaryKey: true),
                Col("year", "SMALLINT"),
                Col("week_number", "TINYINT"),
                Col("start_date", "DATE"),
                Col("end_date", "DATE")
            ],
            indexes:
            [
                Idx("UX_weeks_year_week_number", unique: true, "year", "week_number")
            ]),

        Table(
            "e_cards",
            [
                Col("e_card_id", "BIGINT", identity: true, primaryKey: true),
                Col("title", "NVARCHAR(256)"),
                Col("description", "NVARCHAR(MAX)", nullable: true),
                Col("card_type", "NVARCHAR(64)", nullable: true),
                Col("image_url", "NVARCHAR(1024)", nullable: true),
                Col("redirect_url", "NVARCHAR(1024)", nullable: true),
                Col("is_active", "BIT", defaultExpression: "1"),
                Col("display_order", "INT", nullable: true),
                Col("created_at", "DATETIME2", defaultExpression: "SYSUTCDATETIME()"),
                Col("updated_at", "DATETIME2", nullable: true)
            ]),

        Table(
            "emergency_numbers",
            [
                Col("emergency_number_id", "BIGINT", identity: true, primaryKey: true),
                Col("name", "NVARCHAR(256)"),
                Col("phone_number", "NVARCHAR(64)"),
                Col("category", "NVARCHAR(128)"),
                Col("description", "NVARCHAR(MAX)", nullable: true),
                Col("is_active", "BIT", defaultExpression: "1"),
                Col("display_order", "INT", nullable: true),
                Col("created_at", "DATETIME2", defaultExpression: "SYSUTCDATETIME()"),
                Col("updated_at", "DATETIME2", nullable: true)
            ]),

        Table(
            "service_routes",
            [
                Col("service_route_id", "BIGINT", identity: true, primaryKey: true),
                Col("route_name", "NVARCHAR(256)"),
                Col("departure_location", "NVARCHAR(256)"),
                Col("arrival_location", "NVARCHAR(256)"),
                Col("route_description", "NVARCHAR(MAX)", nullable: true),
                Col("departure_time", "TIME", nullable: true),
                Col("arrival_time", "TIME", nullable: true),
                Col("is_active", "BIT", defaultExpression: "1"),
                Col("display_order", "INT", nullable: true),
                Col("created_at", "DATETIME2", defaultExpression: "SYSUTCDATETIME()"),
                Col("updated_at", "DATETIME2", nullable: true)
            ]),

        Table(
            "today_in_history",
            [
                Col("id", "BIGINT", identity: true, primaryKey: true),
                Col("event_date", "DATETIME2"),
                Col("title", "NVARCHAR(512)"),
                Col("description", "NVARCHAR(MAX)", nullable: true),
                Col("image_url", "NVARCHAR(1024)", nullable: true),
                Col("created_at", "DATETIME2", defaultExpression: "SYSUTCDATETIME()")
            ])
    ];
}
