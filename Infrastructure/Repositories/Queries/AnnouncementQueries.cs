namespace Infrastructure.Repositories.Queries;

public static class AnnouncementQueries
{
    private const string BaseSelect = """
        SELECT
            announcement_id,
            title,
            content,
            cover_image_url,
            author_user_id,
            is_pinned,
            publish_start,
            publish_end,
            view_count,
            created_at,
            updated_at
        FROM announcements
        """;

    public static string GetAll => $"""
        {BaseSelect}
        ORDER BY
            is_pinned DESC,
            created_at DESC;
        """;

    public static string GetPublished => $"""
        {BaseSelect}
        WHERE publish_start <= GETUTCDATE()
          AND (
                publish_end IS NULL
                OR publish_end >= GETUTCDATE()
              )
        ORDER BY
            is_pinned DESC,
            created_at DESC;
        """;

    public static string GetById => $"""
        {BaseSelect}
        WHERE announcement_id = @AnnouncementId;
        """;

    // Not: sona eklenen SELECT SCOPE_IDENTITY() sayesinde
    // ExecuteScalarAsync<long> ile yeni kaydın Id'sini geri alabiliyoruz.
    public const string Create = """
        INSERT INTO announcements
        (
            title,
            content,
            cover_image_url,
            author_user_id,
            is_pinned,
            publish_start,
            publish_end,
            view_count,
            created_at,
            updated_at
        )
        VALUES
        (
            @Title,
            @Content,
            @CoverImageUrl,
            @AuthorUserId,
            @IsPinned,
            @PublishStart,
            @PublishEnd,
            0,
            GETUTCDATE(),
            GETUTCDATE()
        );

        SELECT CAST(SCOPE_IDENTITY() AS BIGINT);
        """;

    public const string Update = """
        UPDATE announcements
        SET
            title = @Title,
            content = @Content,
            cover_image_url = @CoverImageUrl,
            author_user_id = @AuthorUserId,
            is_pinned = @IsPinned,
            publish_start = @PublishStart,
            publish_end = @PublishEnd,
            updated_at = GETUTCDATE()
        WHERE announcement_id = @AnnouncementId;
        """;

    public const string Delete = """
        DELETE FROM announcements
        WHERE announcement_id = @AnnouncementId;
        """;
}