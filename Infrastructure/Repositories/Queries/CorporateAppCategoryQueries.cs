namespace Infrastructure.Repositories.Queries;

public static class CorporateAppCategoryQueries
{
    private const string SelectColumns = """
        SELECT
            corporate_app_category_id,
            name,
            description,
            icon_url,
            color_key,
            sort_order,
            is_active,
            created_at,
            updated_at
        FROM corporate_app_categories
        """;

    public const string GetAll = $"{SelectColumns} ORDER BY sort_order ASC, name ASC;";

    public const string GetById = $"{SelectColumns} WHERE corporate_app_category_id = @CorporateAppCategoryId;";

    public const string GetByName = $"{SelectColumns} WHERE name = @Name;";

    public const string Create = """
        INSERT INTO corporate_app_categories (name, description, icon_url, color_key, sort_order, is_active)
        OUTPUT INSERTED.corporate_app_category_id
        VALUES (@Name, @Description, @IconUrl, @ColorKey, @SortOrder, @IsActive);
        """;

    public const string Update = """
        UPDATE corporate_app_categories
        SET name = @Name,
            description = @Description,
            icon_url = @IconUrl,
            color_key = @ColorKey,
            sort_order = @SortOrder,
            is_active = @IsActive,
            updated_at = SYSUTCDATETIME()
        WHERE corporate_app_category_id = @CorporateAppCategoryId;
        """;

    public const string SoftDelete = """
        UPDATE corporate_app_categories
        SET is_active = 0, updated_at = SYSUTCDATETIME()
        WHERE corporate_app_category_id = @CorporateAppCategoryId;
        """;
}
