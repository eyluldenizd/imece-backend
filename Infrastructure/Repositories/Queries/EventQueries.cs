namespace Infrastructure.Queries;

public static class EventQueries
{
    public const string GetAll = """
        SELECT
            event_id,
            title,
            description,
            event_type,
            location,
            cover_image_url,
            start_datetime,
            end_datetime,
            is_all_day,
            created_by,
            created_at
        FROM events
        ORDER BY start_datetime DESC;
        """;


    public const string GetById = """
        SELECT
            event_id,
            title,
            description,
            event_type,
            location,
            cover_image_url,
            start_datetime,
            end_datetime,
            is_all_day,
            created_by,
            created_at
        FROM events
        WHERE event_id = @EventId;
        """;


    public const string Create = """
        INSERT INTO events
        (
            title,
            description,
            event_type,
            location,
            cover_image_url,
            start_datetime,
            end_datetime,
            is_all_day,
            created_by,
            created_at
        )
        VALUES
        (
            @Title,
            @Description,
            @EventType,
            @Location,
            @CoverImageUrl,
            @StartDatetime,
            @EndDatetime,
            @IsAllDay,
            @CreatedBy,
            GETDATE()
        );
        """;


    public const string Update = """
        UPDATE events
        SET
            title = @Title,
            description = @Description,
            event_type = @EventType,
            location = @Location,
            cover_image_url = @CoverImageUrl,
            start_datetime = @StartDatetime,
            end_datetime = @EndDatetime,
            is_all_day = @IsAllDay
        WHERE event_id = @EventId;
        """;


    public const string Delete = """
        DELETE FROM events
        WHERE event_id = @EventId;
        """;
}