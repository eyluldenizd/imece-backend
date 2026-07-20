namespace Infrastructure.Repositories.Queries;

public static class ServiceLocationTypeQueries
{
    private const string SelectColumns = """
        SELECT
            service_location_type_id,
            name,
            description,
            icon_url,
            color_key,
            sort_order,
            is_active,
            created_at,
            updated_at
        FROM service_location_types
        """;

    public const string GetAll = $"{SelectColumns} ORDER BY sort_order ASC, name ASC;";

    public const string GetById = $"{SelectColumns} WHERE service_location_type_id = @ServiceLocationTypeId;";

    public const string GetByName = $"{SelectColumns} WHERE name = @Name;";

    public const string Create = """
        INSERT INTO service_location_types (name, description, icon_url, color_key, sort_order, is_active)
        OUTPUT INSERTED.service_location_type_id
        VALUES (@Name, @Description, @IconUrl, @ColorKey, @SortOrder, @IsActive);
        """;

    public const string Update = """
        UPDATE service_location_types
        SET name = @Name,
            description = @Description,
            icon_url = @IconUrl,
            color_key = @ColorKey,
            sort_order = @SortOrder,
            is_active = @IsActive,
            updated_at = SYSUTCDATETIME()
        WHERE service_location_type_id = @ServiceLocationTypeId;
        """;

    public const string SoftDelete = """
        UPDATE service_location_types
        SET is_active = 0, updated_at = SYSUTCDATETIME()
        WHERE service_location_type_id = @ServiceLocationTypeId;
        """;
}
