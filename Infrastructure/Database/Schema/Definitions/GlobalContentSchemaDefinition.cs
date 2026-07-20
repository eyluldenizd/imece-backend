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

                ..OrganizationScopeColumns(),

                Col("created_at", "DATETIME2", defaultExpression: "SYSUTCDATETIME()"),

                Col("updated_at", "DATETIME2", defaultExpression: "SYSUTCDATETIME()")

            ],

            foreignKeys: OrganizationScopeForeignKeys("services")),



        Table(

            "corporate_app_categories",

            [

                Col("corporate_app_category_id", "INT", identity: true, primaryKey: true),

                Col("name", "NVARCHAR(128)"),

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

                Idx("UX_corporate_app_categories_name", unique: true, "name"),

                Idx("IX_corporate_app_categories_sort_order", unique: false, "sort_order")

            ]),



        Table(

            "corporate_apps",

            [

                Col("app_id", "BIGINT", identity: true, primaryKey: true),

                Col("title", "NVARCHAR(256)"),

                Col("description", "NVARCHAR(MAX)", nullable: true),

                Col("url", "NVARCHAR(1024)"),

                Col("corporate_app_category_id", "INT", nullable: true),

                Col("category", "NVARCHAR(128)", nullable: true),

                Col("icon_url", "NVARCHAR(1024)", nullable: true),

                Col("is_active", "BIT", defaultExpression: "1"),

                ..OrganizationScopeColumns()

            ],

            indexes:

            [

                Idx("IX_corporate_apps_category_id", unique: false, "corporate_app_category_id")

            ],

            foreignKeys:

            [

                ..OrganizationScopeForeignKeys("corporate_apps"),

                Fk("FK_corporate_apps_categories", "corporate_app_category_id", "corporate_app_categories", "corporate_app_category_id")

            ]),



        Table(

            "communication_channel_types",

            [

                Col("communication_channel_type_id", "INT", identity: true, primaryKey: true),

                Col("name", "NVARCHAR(128)"),

                Col("code", "NVARCHAR(64)"),

                Col("icon_url", "NVARCHAR(1024)", nullable: true),

                Col("is_active", "BIT", defaultExpression: "1"),

                Col("sort_order", "INT", defaultExpression: "0"),

                Col("created_at", "DATETIME2", defaultExpression: "SYSUTCDATETIME()"),

                Col("updated_at", "DATETIME2", defaultExpression: "SYSUTCDATETIME()")

            ],

            indexes:

            [

                Idx("UX_communication_channel_types_code", unique: true, "code"),

                Idx("IX_communication_channel_types_sort_order", unique: false, "sort_order")

            ]),



        Table(

            "communication_channels",

            [

                Col("channel_id", "BIGINT", identity: true, primaryKey: true),

                Col("channel_name", "NVARCHAR(256)"),

                Col("type", "NVARCHAR(64)"),

                Col("communication_channel_type_id", "INT", nullable: true),

                Col("address_url", "NVARCHAR(1024)"),

                Col("department_in_charge", "NVARCHAR(256)", nullable: true),

                Col("description", "NVARCHAR(MAX)", nullable: true),

                Col("icon", "NVARCHAR(256)", nullable: true),

                Col("sort_order", "INT", defaultExpression: "0"),

                Col("is_active", "BIT", defaultExpression: "1"),

                ..OrganizationScopeColumns(),

                Col("created_at", "DATETIME2", defaultExpression: "SYSUTCDATETIME()"),

                Col("updated_at", "DATETIME2", defaultExpression: "SYSUTCDATETIME()")

            ],

            indexes:

            [

                Idx("IX_communication_channels_company_id", unique: false, "company_id"),

                Idx("IX_communication_channels_is_active", unique: false, "is_active"),

                Idx("IX_communication_channels_type_id", unique: false, "communication_channel_type_id")

            ],

            foreignKeys:

            [

                ..OrganizationScopeForeignKeys("communication_channels"),

                Fk("FK_communication_channels_types", "communication_channel_type_id", "communication_channel_types", "communication_channel_type_id")

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

                Col("is_active", "BIT", defaultExpression: "1"),

                ..OrganizationScopeColumns()

            ],

            foreignKeys: OrganizationScopeForeignKeys("campaigns")),



        Table(

            "dishes",

            [

                Col("dish_id", "INT", identity: true, primaryKey: true),

                Col("dish_name", "NVARCHAR(256)"),

                Col("dish_category_id", "INT", nullable: true),

                Col("category", "NVARCHAR(128)"),

                Col("description", "NVARCHAR(1024)", nullable: true),

                Col("image_url", "NVARCHAR(1024)", nullable: true),

                Col("is_active", "BIT", defaultExpression: "1"),

                Col("created_at", "DATETIME2", defaultExpression: "SYSUTCDATETIME()"),

                Col("updated_at", "DATETIME2", defaultExpression: "SYSUTCDATETIME()")

            ],

            foreignKeys:

            [

                Fk("FK_dishes_dish_categories", "dish_category_id", "dish_categories", "dish_category_id")

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

                ..OrganizationScopeColumns(),

                Col("created_at", "DATETIME2", defaultExpression: "SYSUTCDATETIME()"),

                Col("updated_at", "DATETIME2", nullable: true)

            ],

            foreignKeys: OrganizationScopeForeignKeys("e_cards")),



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

                ..OrganizationScopeColumns(),

                Col("created_at", "DATETIME2", defaultExpression: "SYSUTCDATETIME()"),

                Col("updated_at", "DATETIME2", nullable: true)

            ],

            foreignKeys: OrganizationScopeForeignKeys("emergency_numbers")),



        Table(

            "service_routes",

            [

                Col("service_route_id", "BIGINT", identity: true, primaryKey: true),

                Col("route_name", "NVARCHAR(256)"),

                Col("departure_location", "NVARCHAR(256)"),

                Col("arrival_location", "NVARCHAR(256)"),

                Col("departure_location_id", "BIGINT", nullable: true),

                Col("arrival_location_id", "BIGINT", nullable: true),

                Col("route_description", "NVARCHAR(MAX)", nullable: true),

                Col("departure_time", "TIME", nullable: true),

                Col("arrival_time", "TIME", nullable: true),

                Col("is_active", "BIT", defaultExpression: "1"),

                Col("display_order", "INT", nullable: true),

                Col("created_at", "DATETIME2", defaultExpression: "SYSUTCDATETIME()"),

                Col("updated_at", "DATETIME2", nullable: true)

            ],

            foreignKeys:

            [

                Fk("FK_service_routes_departure_location", "departure_location_id", "service_locations", "service_location_id"),

                Fk("FK_service_routes_arrival_location", "arrival_location_id", "service_locations", "service_location_id")

            ]),



        Table(

            "today_in_history",

            [

                Col("id", "BIGINT", identity: true, primaryKey: true),

                Col("event_date", "DATETIME2"),

                Col("title", "NVARCHAR(512)"),

                Col("description", "NVARCHAR(MAX)", nullable: true),

                Col("image_url", "NVARCHAR(1024)", nullable: true),

                ..OrganizationScopeColumns(),

                Col("created_at", "DATETIME2", defaultExpression: "SYSUTCDATETIME()")

            ],

            foreignKeys: OrganizationScopeForeignKeys("today_in_history"))

    ];

}


