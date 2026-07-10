using Infrastructure.Entities;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Mappers;

public static class AnnouncementMapper
{
    public static Announcements Map(SqlDataReader reader)
    {
        return new Announcements
        {
            AnnouncementId = reader.GetInt64(
                reader.GetOrdinal("announcement_id")),

            Title = reader.GetString(
                reader.GetOrdinal("title")),

            Content = reader.GetString(
                reader.GetOrdinal("content")),

            CoverImageUrl = reader.IsDBNull(
                reader.GetOrdinal("cover_image_url"))
                    ? null
                    : reader.GetString(
                        reader.GetOrdinal("cover_image_url")),

            AuthorUserId = reader.GetInt32(
                reader.GetOrdinal("author_user_id")),

            IsPinned = reader.GetBoolean(
                reader.GetOrdinal("is_pinned")),

            PublishStart = reader.GetDateTime(
                reader.GetOrdinal("publish_start")),

            PublishEnd = reader.IsDBNull(
                reader.GetOrdinal("publish_end"))
                    ? null
                    : reader.GetDateTime(
                        reader.GetOrdinal("publish_end")),

            ViewCount = reader.GetInt32(
                reader.GetOrdinal("view_count")),

            CreatedAt = reader.GetDateTime(
                reader.GetOrdinal("created_at")),

            UpdatedAt = reader.GetDateTime(
                reader.GetOrdinal("updated_at"))
        };
    }
}