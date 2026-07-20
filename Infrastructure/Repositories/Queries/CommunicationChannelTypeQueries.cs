namespace Infrastructure.Repositories.Queries;

public static class CommunicationChannelTypeQueries
{
    // Entity uses DbColumn snake_case names — do not Pascal-alias.
    private const string SelectColumns = """
        SELECT
            communication_channel_type_id,
            name,
            code,
            icon_url,
            is_active,
            sort_order,
            created_at,
            updated_at
        FROM communication_channel_types
        """;

    public const string GetAll = $"{SelectColumns} ORDER BY sort_order ASC, name ASC;";

    public const string GetById = $"{SelectColumns} WHERE communication_channel_type_id = @CommunicationChannelTypeId;";

    public const string GetByCode = $"{SelectColumns} WHERE code = @Code;";

    public const string Create = """
        INSERT INTO communication_channel_types (name, code, icon_url, sort_order, is_active)
        OUTPUT INSERTED.communication_channel_type_id
        VALUES (@Name, @Code, @IconUrl, @SortOrder, @IsActive);
        """;

    public const string Update = """
        UPDATE communication_channel_types
        SET name = @Name,
            code = @Code,
            icon_url = @IconUrl,
            sort_order = @SortOrder,
            is_active = @IsActive,
            updated_at = SYSUTCDATETIME()
        WHERE communication_channel_type_id = @CommunicationChannelTypeId;
        """;

    public const string SoftDelete = """
        UPDATE communication_channel_types
        SET is_active = 0, updated_at = SYSUTCDATETIME()
        WHERE communication_channel_type_id = @CommunicationChannelTypeId;
        """;
}
