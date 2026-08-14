namespace Infrastructure.Repositories.Queries;

public static class EmergencyNumberCategoryQueries
{
    private const string SelectColumns = """
        SELECT
            emergency_number_category_id,
            name,
            description,
            icon_url,
            color_key,
            sort_order,
            is_active,
            created_at,
            updated_at
        FROM emergency_number_categories
        """;

    public const string GetAll = $"{SelectColumns} ORDER BY sort_order ASC, name ASC;";

    public const string GetById = $"{SelectColumns} WHERE emergency_number_category_id = @EmergencyNumberCategoryId;";

    public const string GetByName = $"{SelectColumns} WHERE name = @Name;";

    public const string Create = """
        INSERT INTO emergency_number_categories (name, description, icon_url, color_key, sort_order, is_active)
        OUTPUT INSERTED.emergency_number_category_id
        VALUES (@Name, @Description, @IconUrl, @ColorKey, @SortOrder, @IsActive);
        """;

    public const string Update = """
        UPDATE emergency_number_categories
        SET name = @Name,
            description = @Description,
            icon_url = @IconUrl,
            color_key = @ColorKey,
            sort_order = @SortOrder,
            is_active = @IsActive,
            updated_at = SYSUTCDATETIME()
        WHERE emergency_number_category_id = @EmergencyNumberCategoryId;
        """;

    public const string SoftDelete = """
        UPDATE emergency_number_categories
        SET is_active = 0, updated_at = SYSUTCDATETIME()
        WHERE emergency_number_category_id = @EmergencyNumberCategoryId;
        """;

    public const string Delete = "DELETE FROM emergency_number_categories WHERE emergency_number_category_id = @EmergencyNumberCategoryId;";
}
