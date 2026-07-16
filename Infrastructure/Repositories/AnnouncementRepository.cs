using Infrastructure.Data;
using Infrastructure.Entities;
using Infrastructure.Repositories.Queries;
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
        return _dbManager.QueryAsync<Announcements>(
            AnnouncementQueries.GetPublished,
            cancellationToken: cancellationToken);
    }

    public Task<List<Announcements>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return _dbManager.QueryAsync<Announcements>(
            AnnouncementQueries.GetAll,
            cancellationToken: cancellationToken);
    }

    public Task<Announcements?> GetByIdAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        var parameters = new[]
        {
            new SqlParameter("@AnnouncementId", SqlDbType.BigInt) { Value = id }
        };

        return _dbManager.QueryFirstOrDefaultAsync<Announcements>(
            AnnouncementQueries.GetById,
            parameters,
            cancellationToken);
    }

    public async Task<long> CreateAsync(
        Announcements announcement,
        CancellationToken cancellationToken = default)
    {
        var parameters = new[]
        {
            new SqlParameter("@Title", announcement.Title),
            new SqlParameter("@Content", announcement.Content),
            new SqlParameter("@CoverImageUrl", (object?)announcement.CoverImageUrl ?? DBNull.Value),
            //new SqlParameter("@AuthorUserId", announcement.AuthorUserId),
            new SqlParameter("@IsPinned", announcement.IsPinned),
            new SqlParameter("@PublishStart", announcement.PublishStart),
            new SqlParameter("@PublishEnd", (object?)announcement.PublishEnd ?? DBNull.Value)
        };

        var newId = await _dbManager.ExecuteScalarAsync<long>(
            AnnouncementQueries.Create,
            parameters,
            cancellationToken);

        return newId;
    }

    public Task<int> UpdateAsync(
        Announcements announcement,
        CancellationToken cancellationToken = default)
    {
        var parameters = new[]
        {
            new SqlParameter("@AnnouncementId", announcement.AnnouncementId),
            new SqlParameter("@Title", announcement.Title),
            new SqlParameter("@Content", announcement.Content),
            new SqlParameter("@CoverImageUrl", (object?)announcement.CoverImageUrl ?? DBNull.Value),
            //new SqlParameter("@AuthorUserId", announcement.AuthorUserId),
            new SqlParameter("@IsPinned", announcement.IsPinned),
            new SqlParameter("@PublishStart", announcement.PublishStart),
            new SqlParameter("@PublishEnd", (object?)announcement.PublishEnd ?? DBNull.Value)
        };

        return _dbManager.ExecuteAsync(
            AnnouncementQueries.Update,
            parameters,
            cancellationToken);
    }

    public Task<int> DeleteAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        var parameters = new[]
        {
            new SqlParameter("@AnnouncementId", SqlDbType.BigInt) { Value = id }
        };

        return _dbManager.ExecuteAsync(
            AnnouncementQueries.Delete,
            parameters,
            cancellationToken);
    }
}