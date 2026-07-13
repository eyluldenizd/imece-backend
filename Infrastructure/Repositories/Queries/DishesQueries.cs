namespace Infrastructure.Repositories.Queries;

public static class DishesQueries
{
    private const string BaseSelect = """
        SELECT
            dish_id,
            dish_name,
            category,
            is_active,
            created_at
        FROM dishes
        """;

    public const string GetAll = BaseSelect + """
        
        ORDER BY dish_name ASC;
        """;

    public const string GetById = BaseSelect + """
        
        WHERE dish_id = @DishId;
        """;

    public const string Delete = """
        DELETE FROM dishes
        WHERE dish_id = @DishId;
        """;
}