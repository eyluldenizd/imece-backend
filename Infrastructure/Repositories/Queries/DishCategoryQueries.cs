namespace Infrastructure.Repositories.Queries;

public static class DishCategoryQueries
{
    private const string SelectColumns = """
        SELECT
            dish_category_id AS DishCategoryId,
            name AS Name,
            code AS Code,
            description AS Description,
            sort_order AS SortOrder,
            is_active AS IsActive,
            created_at AS CreatedAt,
            updated_at AS UpdatedAt
        FROM dish_categories
        """;

    public const string GetAll = $"{SelectColumns} ORDER BY sort_order ASC, name ASC;";

    public const string GetActive = $"{SelectColumns} WHERE is_active = 1 ORDER BY sort_order ASC, name ASC;";

    public const string GetById = $"{SelectColumns} WHERE dish_category_id = @DishCategoryId;";

    public const string GetByCode = $"{SelectColumns} WHERE code = @Code;";

    public const string Create = """
        INSERT INTO dish_categories (name, code, description, sort_order, is_active)
        OUTPUT INSERTED.dish_category_id
        VALUES (@Name, @Code, @Description, @SortOrder, @IsActive);
        """;

    public const string Update = """
        UPDATE dish_categories
        SET name = @Name,
            code = @Code,
            description = @Description,
            sort_order = @SortOrder,
            is_active = @IsActive,
            updated_at = SYSDATETIME()
        WHERE dish_category_id = @DishCategoryId;
        """;

    public const string SoftDelete = """
        UPDATE dish_categories
        SET is_active = 0, updated_at = SYSDATETIME()
        WHERE dish_category_id = @DishCategoryId;
        """;
}
