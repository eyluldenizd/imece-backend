namespace Infrastructure.Queries;

public static class EmergencyNumberQueries
{
    public const string GetAll = """
        SELECT
            emergency_number_id,
            name,
            phone_number,
            category,
            description,
            is_active,
            display_order,
            created_at,
            updated_at
        FROM emergency_numbers
        ORDER BY display_order ASC, name ASC;
        """;


    public const string GetById = """
        SELECT
            emergency_number_id,
            name,
            phone_number,
            category,
            description,
            is_active,
            display_order,
            created_at,
            updated_at
        FROM emergency_numbers
        WHERE emergency_number_id = @EmergencyNumberId;
        """;


    public const string Create = """
        INSERT INTO emergency_numbers
        (
            name,
            phone_number,
            category,
            description,
            is_active,
            display_order,
            created_at
        )
        VALUES
        (
            @Name,
            @PhoneNumber,
            @Category,
            @Description,
            @IsActive,
            @DisplayOrder,
            GETDATE()
        );
        """;


    public const string Update = """
        UPDATE emergency_numbers
        SET
            name = @Name,
            phone_number = @PhoneNumber,
            category = @Category,
            description = @Description,
            is_active = @IsActive,
            display_order = @DisplayOrder,
            updated_at = GETDATE()
        WHERE emergency_number_id = @EmergencyNumberId;
        """;


    public const string Delete = """
        DELETE FROM emergency_numbers
        WHERE emergency_number_id = @EmergencyNumberId;
        """;
}