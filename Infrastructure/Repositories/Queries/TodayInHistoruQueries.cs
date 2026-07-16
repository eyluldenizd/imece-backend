namespace Infrastructure.Queries;

public static class TodayInHistoryQueries
{
    public const string GetAll = """
        SELECT
            id,
            event_date,
            title,
            description,
            image_url,
            created_at
        FROM today_in_history
        ORDER BY event_date DESC;
        """;


    public const string Create = """
        INSERT INTO today_in_history
        (
            event_date,
            title,
            description,
            image_url,
            created_at
        )
        VALUES
        (
            @EventDate,
            @Title,
            @Description,
            @ImageUrl,
            GETDATE()
        );
        """;


    public const string Update = """
        UPDATE today_in_history
        SET
            event_date = @EventDate,
            title = @Title,
            description = @Description,
            image_url = @ImageUrl
        WHERE id = @Id;
        """;


    public const string Delete = """
        DELETE FROM today_in_history
        WHERE id = @Id;
        """;
}