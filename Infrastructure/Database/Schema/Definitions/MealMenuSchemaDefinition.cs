using static Infrastructure.Database.Schema.SchemaTableBuilder;

namespace Infrastructure.Database.Schema.Definitions;

public sealed class MealMenuSchemaDefinition : ISchemaDefinition
{
    public string FeatureName => "MealMenu";

    public IReadOnlyList<TableDefinition> Tables { get; } =
    [
        Table(
            "dish_categories",
            [
                Col("dish_category_id", "INT", identity: true, primaryKey: true),
                Col("name", "NVARCHAR(128)"),
                Col("code", "NVARCHAR(64)"),
                Col("description", "NVARCHAR(512)", nullable: true),
                Col("sort_order", "INT", defaultExpression: "0"),
                Col("is_active", "BIT", defaultExpression: "1"),
                Col("created_at", "DATETIME2", defaultExpression: "SYSUTCDATETIME()"),
                Col("updated_at", "DATETIME2", defaultExpression: "SYSUTCDATETIME()")
            ],
            indexes:
            [
                Idx("UX_dish_categories_code", unique: true, "code"),
                Idx("IX_dish_categories_sort_order", unique: false, "sort_order")
            ]),

        Table(
            "weekly_menus",
            [
                Col("menu_id", "BIGINT", identity: true, primaryKey: true),
                Col("company_id", "INT"),
                Col("menu_code", "NVARCHAR(16)"),
                Col("year", "INT"),
                Col("month", "INT"),
                Col("week_of_month", "INT"),
                Col("period_start_date", "DATE"),
                Col("period_end_date", "DATE"),
                Col("title", "NVARCHAR(256)", nullable: true),
                Col("is_published", "BIT", defaultExpression: "0"),
                Col("published_at", "DATETIME2", nullable: true),
                Col("is_active", "BIT", defaultExpression: "1"),
                Col("created_by", "INT"),
                Col("created_at", "DATETIME2", defaultExpression: "SYSUTCDATETIME()"),
                Col("updated_at", "DATETIME2", defaultExpression: "SYSUTCDATETIME()")
            ],
            indexes:
            [
                Idx("UX_weekly_menus_company_menu_code", unique: true, "company_id", "menu_code"),
                Idx("IX_weekly_menus_company_id", unique: false, "company_id"),
                Idx("IX_weekly_menus_year_month_week", unique: false, "year", "month", "week_of_month")
            ],
            foreignKeys:
            [
                Fk("FK_weekly_menus_companies", "company_id", "companies", "company_id"),
                Fk("FK_weekly_menus_created_by", "created_by", "users", "user_id")
            ],
            checks:
            [
                new()
                {
                    Name = "CK_weekly_menus_month",
                    Expression = "[month] >= 1 AND [month] <= 12"
                },
                new()
                {
                    Name = "CK_weekly_menus_week_of_month",
                    Expression = "[week_of_month] >= 1 AND [week_of_month] <= 5"
                }
            ]),

        Table(
            "weekly_menu_items",
            [
                Col("menu_item_id", "BIGINT", identity: true, primaryKey: true),
                Col("menu_id", "BIGINT"),
                Col("menu_date", "DATE"),
                Col("dish_category_id", "INT"),
                Col("dish_id", "INT"),
                Col("sort_order", "INT", defaultExpression: "0"),
                Col("notes", "NVARCHAR(512)", nullable: true),
                Col("is_active", "BIT", defaultExpression: "1"),
                Col("created_at", "DATETIME2", defaultExpression: "SYSUTCDATETIME()"),
                Col("updated_at", "DATETIME2", defaultExpression: "SYSUTCDATETIME()")
            ],
            indexes:
            [
                Idx("IX_weekly_menu_items_menu_id", unique: false, "menu_id"),
                Idx("IX_weekly_menu_items_menu_date", unique: false, "menu_date"),
                Idx(
                    "UX_weekly_menu_items_menu_date_category_dish",
                    unique: true,
                    "menu_id",
                    "menu_date",
                    "dish_category_id",
                    "dish_id")
            ],
            foreignKeys:
            [
                Fk("FK_weekly_menu_items_weekly_menus", "menu_id", "weekly_menus", "menu_id"),
                Fk("FK_weekly_menu_items_dish_categories", "dish_category_id", "dish_categories", "dish_category_id"),
                Fk("FK_weekly_menu_items_dishes", "dish_id", "dishes", "dish_id")
            ])
    ];
}
