namespace Infrastructure.Repositories.Queries;

public static class EventQueries
{
    private const string SelectColumns = """
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
        """;

    public const string GetAll = $"""
        {SelectColumns}
        ORDER BY start_datetime ASC;
        """;

    public const string GetUpcoming = $"""
        {SelectColumns}
        WHERE end_datetime >= SYSDATETIME()
        ORDER BY start_datetime ASC;
        """;

    public const string GetById = $"""
        {SelectColumns}
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
            created_by
        )
        OUTPUT INSERTED.event_id
        VALUES
        (
            @Title,
            @Description,
            @EventType,
            @Location,
            @CoverImageUrl,
            @StartDateTime,
            @EndDateTime,
            @IsAllDay,
            @CreatedBy
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
            start_datetime = @StartDateTime,
            end_datetime = @EndDateTime,
            is_all_day = @IsAllDay,
            created_by = @CreatedBy
        WHERE event_id = @EventId;
        """;

    public const string Delete = """
        DELETE FROM events
        WHERE event_id = @EventId;
        """;
}