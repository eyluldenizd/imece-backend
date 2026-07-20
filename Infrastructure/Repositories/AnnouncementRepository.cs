using Infrastructure.Database.DataAccess;
using Infrastructure.Entities;
using Infrastructure.Repositories.Queries;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Infrastructure.Repositories;

public sealed class AnnouncementRepository
{
    private readonly ISqlDataAccess _dataAccess;

    public AnnouncementRepository(ISqlDataAccess dataAccess)
    {
        _dataAccess = dataAccess;
    }

    public Task<List<Announcements>> GetPublishedAsync(
        CompanyListFilter filter,
        CancellationToken cancellationToken = default)
    {
        return _dataAccess.QueryAsync<Announcements>(
            AnnouncementQueries.GetPublished,
            CompanyListFilterParameters.Create(filter),
            cancellationToken);
    }

    public Task<List<Announcements>> GetAllAsync(
        CompanyListFilter filter,
        CancellationToken cancellationToken = default)
    {
        return _dataAccess.QueryAsync<Announcements>(
            AnnouncementQueries.GetAll,
            CompanyListFilterParameters.Create(filter),
            cancellationToken);
    }

    public Task<Announcements?> GetByIdAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        var parameters = new[]
        {
            new SqlParameter("@AnnouncementId", SqlDbType.BigInt) { Value = id }
        };

        return _dataAccess.QueryFirstOrDefaultAsync<Announcements>(
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
            new SqlParameter("@CompanyId", SqlDbType.Int)
            {
                Value = announcement.CompanyId.HasValue
                    ? announcement.CompanyId.Value
                    : DBNull.Value
            },
            new SqlParameter("@ScopeType", SqlDbType.NVarChar, 16)
            {
                Value = announcement.ScopeType
            },
            new SqlParameter("@Title", announcement.Title),
            new SqlParameter("@Content", announcement.Content),
            new SqlParameter("@CoverImageUrl", (object?)announcement.CoverImageUrl ?? DBNull.Value),
            new SqlParameter("@IsPinned", announcement.IsPinned),
            new SqlParameter("@PublishStart", announcement.PublishStart),
            new SqlParameter("@PublishEnd", (object?)announcement.PublishEnd ?? DBNull.Value)
        };

        var newId = await _dataAccess.ExecuteScalarAsync<long>(
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
            new SqlParameter("@CompanyId", SqlDbType.Int)
            {
                Value = announcement.CompanyId.HasValue
                    ? announcement.CompanyId.Value
                    : DBNull.Value
            },
            new SqlParameter("@ScopeType", SqlDbType.NVarChar, 16)
            {
                Value = announcement.ScopeType
            },
            new SqlParameter("@Title", announcement.Title),
            new SqlParameter("@Content", announcement.Content),
            new SqlParameter("@CoverImageUrl", (object?)announcement.CoverImageUrl ?? DBNull.Value),
            new SqlParameter("@IsPinned", announcement.IsPinned),
            new SqlParameter("@PublishStart", announcement.PublishStart),
            new SqlParameter("@PublishEnd", (object?)announcement.PublishEnd ?? DBNull.Value)
        };

        return _dataAccess.ExecuteAsync(
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

        return _dataAccess.ExecuteAsync(
            AnnouncementQueries.Delete,
            parameters,
            cancellationToken);
    }
}
