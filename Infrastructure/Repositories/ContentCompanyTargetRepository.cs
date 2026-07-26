using Infrastructure.Database.DataAccess;
using Infrastructure.Repositories.Queries;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Repositories;

public sealed class ContentCompanyTargetRepository
{
    private readonly ISqlDataAccess _dataAccess;

    public ContentCompanyTargetRepository(ISqlDataAccess dataAccess)
    {
        _dataAccess = dataAccess;
    }

    public Task<List<int>> GetCompanyIdsAsync(
        string contentType,
        long contentId,
        CancellationToken cancellationToken = default)
    {
        var parameters = new List<SqlParameter>
        {
            new("@ContentType", contentType),
            new("@ContentId", contentId),
        };

        return _dataAccess.QueryAsync<int>(ContentCompanyTargetQueries.GetByContent, parameters, cancellationToken);
    }

    public Task<List<ContentCompanyTargetRow>> GetByContentTypeAsync(
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var parameters = new List<SqlParameter>
        {
            new("@ContentType", contentType),
        };

        return _dataAccess.QueryAsync<ContentCompanyTargetRow>(
            ContentCompanyTargetQueries.GetByContentType,
            parameters,
            cancellationToken);
    }

    public async Task ReplaceAsync(
        string contentType,
        long contentId,
        IReadOnlyCollection<int> companyIds,
        CancellationToken cancellationToken = default)
    {
        var deleteParameters = new List<SqlParameter>
        {
            new("@ContentType", contentType),
            new("@ContentId", contentId),
        };

        await _dataAccess.ExecuteAsync(
            ContentCompanyTargetQueries.DeleteByContent,
            deleteParameters,
            cancellationToken);

        foreach (var companyId in companyIds.Distinct().OrderBy(id => id))
        {
            var insertParameters = new List<SqlParameter>
            {
                new("@ContentType", contentType),
                new("@ContentId", contentId),
                new("@CompanyId", companyId),
            };

            await _dataAccess.ExecuteAsync(
                ContentCompanyTargetQueries.Insert,
                insertParameters,
                cancellationToken);
        }
    }

    public Task DeleteByContentAsync(
        string contentType,
        long contentId,
        CancellationToken cancellationToken = default)
    {
        var parameters = new List<SqlParameter>
        {
            new("@ContentType", contentType),
            new("@ContentId", contentId),
        };

        return _dataAccess.ExecuteAsync(
            ContentCompanyTargetQueries.DeleteByContent,
            parameters,
            cancellationToken);
    }
}

public sealed class ContentCompanyTargetRow
{
    public long ContentId { get; set; }
    public int CompanyId { get; set; }
}
