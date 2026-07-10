using Infrastructure.Data;
using Infrastructure.Entities;
using Infrastructure.Mappers;
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
        return _dbManager.QueryAsync(
            AnnouncementQueries.GetPublished,
            AnnouncementMapper.Map,
            cancellationToken: cancellationToken);
    }

    public Task<List<Announcements>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return _dbManager.QueryAsync(
            AnnouncementQueries.GetAll,
            AnnouncementMapper.Map,
            cancellationToken: cancellationToken);
    }

    public Task<Announcements?> GetByIdAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        var parameters = new[]
        {
            new SqlParameter(
                "@AnnouncementId",
                SqlDbType.BigInt)
            {
                Value = id
            }
        };

        return _dbManager.QueryFirstOrDefaultAsync(
            AnnouncementQueries.GetById,
            AnnouncementMapper.Map,
            parameters,
            cancellationToken);
    }

    public Task<int> DeleteAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        var parameters = new[]
        {
            new SqlParameter(
                "@AnnouncementId",
                SqlDbType.BigInt)
            {
                Value = id
            }
        };

        return _dbManager.ExecuteAsync(
            AnnouncementQueries.Delete,
            parameters,
            cancellationToken);
    }
}