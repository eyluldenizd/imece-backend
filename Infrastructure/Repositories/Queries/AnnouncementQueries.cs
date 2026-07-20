namespace Infrastructure.Repositories.Queries;



public static class AnnouncementQueries

{

    private const string BaseSelect = """

        SELECT

            announcement_id,

            company_id,

            scope_type,

            title,

            content,

            cover_image_url,

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

        WHERE {CompanyScopeSql.ListFilter}

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

          AND {CompanyScopeSql.ListFilter}

        ORDER BY

            is_pinned DESC,

            created_at DESC;

        """;



    public static string GetById => $"""

        {BaseSelect}

        WHERE announcement_id = @AnnouncementId;

        """;



    public const string Create = """

        INSERT INTO announcements

        (

            company_id,

            scope_type,

            title,

            content,

            cover_image_url,

            is_pinned,

            publish_start,

            publish_end,

            view_count,

            created_at,

            updated_at

        )

        VALUES

        (

            @CompanyId,

            @ScopeType,

            @Title,

            @Content,

            @CoverImageUrl,

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

            company_id = @CompanyId,

            scope_type = @ScopeType,

            title = @Title,

            content = @Content,

            cover_image_url = @CoverImageUrl,

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

