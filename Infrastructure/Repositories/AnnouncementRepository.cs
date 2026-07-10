using Infrastructure.Data;
using Infrastructure.Entities;
using Infrastructure.Mappers;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Infrastructure.Repositories;

public sealed class AnnouncementRepository
{
    private readonly DbManager _dbManager;

    public AnnouncementRepository(DbManager dbManager)
    {
        _dbManager = dbManager;
    }

    public Task<List<Announcements>> GetPublishedAsync(
        CancellationToken cancellationToken = default)
    {
        const string sql = """
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

        return _dbManager.QueryAsync(
            sql,
            AnnouncementMapper.Map,
            cancellationToken: cancellationToken);
    }

    public Task<List<Announcements>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        const string sql = """
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

        return _dbManager.QueryAsync(
            sql,
            AnnouncementMapper.Map,
            cancellationToken: cancellationToken);
    }

    public Task<Announcements?> GetByIdAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
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

        var parameters = new[]
        {
            new SqlParameter("@AnnouncementId", SqlDbType.BigInt)
            {
                Value = id
            }
        };

        return _dbManager.QuerySingleOrDefaultAsync(
            sql,
            AnnouncementMapper.Map,
            parameters,
            cancellationToken);
    }
}