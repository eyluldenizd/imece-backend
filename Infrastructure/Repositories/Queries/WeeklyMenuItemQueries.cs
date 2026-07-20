namespace Infrastructure.Repositories.Queries;

public static class WeeklyMenuItemQueries
{
    private const string SelectColumns = """
        SELECT
            menu_item_id AS MenuItemId,
            menu_id AS MenuId,
            menu_date AS MenuDate,
            dish_category_id AS DishCategoryId,
            dish_id AS DishId,
            sort_order AS SortOrder,
            notes AS Notes,
            is_active AS IsActive,
            created_at AS CreatedAt,
            updated_at AS UpdatedAt
        FROM weekly_menu_items
        """;

    public const string GetByMenuId = $"""
        {SelectColumns}
        WHERE menu_id = @MenuId AND is_active = 1
        ORDER BY menu_date ASC, sort_order ASC, menu_item_id ASC;
        """;

    public const string GetById = $"""
        {SelectColumns}
        WHERE menu_item_id = @MenuItemId AND is_active = 1;
        """;

    public const string GetByIdAndMenuId = $"""
        {SelectColumns}
        WHERE menu_item_id = @MenuItemId
          AND menu_id = @MenuId
          AND is_active = 1;
        """;

    public const string ExistsDuplicate = """
        SELECT TOP 1 1
        FROM weekly_menu_items
        WHERE menu_id = @MenuId
          AND menu_date = @MenuDate
          AND dish_category_id = @DishCategoryId
          AND dish_id = @DishId
          AND is_active = 1
          AND (@ExcludeMenuItemId IS NULL OR menu_item_id <> @ExcludeMenuItemId);
        """;

    public const string Create = """
        INSERT INTO weekly_menu_items (
            menu_id,
            menu_date,
            dish_category_id,
            dish_id,
            sort_order,
            notes,
            is_active
        )
        OUTPUT INSERTED.menu_item_id
        VALUES (
            @MenuId,
            @MenuDate,
            @DishCategoryId,
            @DishId,
            @SortOrder,
            @Notes,
            1
        );
        """;

    public const string Update = """
        UPDATE weekly_menu_items
        SET menu_date = @MenuDate,
            dish_category_id = @DishCategoryId,
            dish_id = @DishId,
            sort_order = @SortOrder,
            notes = @Notes,
            updated_at = SYSDATETIME()
        WHERE menu_item_id = @MenuItemId
          AND menu_id = @MenuId
          AND is_active = 1;
        """;

    public const string SoftDelete = """
        UPDATE weekly_menu_items
        SET is_active = 0, updated_at = SYSDATETIME()
        WHERE menu_item_id = @MenuItemId
          AND menu_id = @MenuId;
        """;
}
