namespace Infrastructure.Queries;

public static class ECardQueries
{
    public const string GetAll = """
        SELECT
            e_card_id,
            title,
            description,
            card_type,
            image_url,
            redirect_url,
            is_active,
            display_order,
            created_at,
            updated_at
        FROM e_cards
        ORDER BY display_order ASC, title ASC;
        """;


    public const string GetById = """
        SELECT
            e_card_id,
            title,
            description,
            card_type,
            image_url,
            redirect_url,
            is_active,
            display_order,
            created_at,
            updated_at
        FROM e_cards
        WHERE e_card_id = @ECardId;
        """;


    public const string Create = """
        INSERT INTO e_cards
        (
            title,
            description,
            card_type,
            image_url,
            redirect_url,
            is_active,
            display_order,
            created_at
        )
        VALUES
        (
            @Title,
            @Description,
            @CardType,
            @ImageUrl,
            @RedirectUrl,
            @IsActive,
            @DisplayOrder,
            GETDATE()
        );
        """;


    public const string Update = """
        UPDATE e_cards
        SET
            title = @Title,
            description = @Description,
            card_type = @CardType,
            image_url = @ImageUrl,
            redirect_url = @RedirectUrl,
            is_active = @IsActive,
            display_order = @DisplayOrder,
            updated_at = GETDATE()
        WHERE e_card_id = @ECardId;
        """;


    public const string Delete = """
        DELETE FROM e_cards
        WHERE e_card_id = @ECardId;
        """;
}