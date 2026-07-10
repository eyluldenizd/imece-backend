using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.Queries;

internal static class AnnouncementQueries
{
    public const string GetPublished = """
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
        WHERE publish_start <= SYSDATETIME()
          AND (
                publish_end IS NULL
                OR publish_end >= SYSDATETIME()
              )
        ORDER BY is_pinned DESC, publish_start DESC;
        """;

    public const string GetAll = """
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
        ORDER BY created_at DESC;
        """;

    public const string GetById = """
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
        WHERE announcement_id = @AnnouncementId;
        """;

    public const string Delete = """
        DELETE FROM announcements
        WHERE announcement_id = @AnnouncementId;
        """;
}