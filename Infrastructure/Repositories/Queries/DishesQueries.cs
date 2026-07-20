namespace Infrastructure.Repositories.Queries;

public static class DishesQueries
{
    // Column names must match DbColumn attributes on Dishes (SqlDataAccess maps by attribute name).
    private const string SelectColumns = """
        SELECT
            dish_id,
            dish_name,
            dish_category_id,
            category,
            description,
            image_url,
            is_active,
            created_at,
            updated_at
        FROM dishes
        """;

    public const string GetAll = $"{SelectColumns} ORDER BY dish_name ASC;";

    public const string GetActive = $"{SelectColumns} WHERE is_active = 1 ORDER BY dish_name ASC;";

    public const string GetById = $"{SelectColumns} WHERE dish_id = @DishId;";

    public const string Create = """
        INSERT INTO dishes (dish_name, dish_category_id, category, description, image_url, is_active)
        OUTPUT INSERTED.dish_id
        VALUES (@DishName, @DishCategoryId, @Category, @Description, @ImageUrl, @IsActive);
        """;

    public const string Update = """
        UPDATE dishes
        SET dish_name = @DishName,
            dish_category_id = @DishCategoryId,
            category = @Category,
            description = @Description,
            image_url = @ImageUrl,
            is_active = @IsActive,
            updated_at = SYSDATETIME()
        WHERE dish_id = @DishId;
        """;

    public const string SoftDelete = """
        UPDATE dishes
        SET is_active = 0, updated_at = SYSDATETIME()
        WHERE dish_id = @DishId;
        """;
}
