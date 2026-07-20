namespace Infrastructure.Database.Backfill;

public interface IContentScopeBackfillService
{
    Task<ContentScopeBackfillReport> RunAsync(CancellationToken cancellationToken = default);
}

public sealed class ContentScopeBackfillReport
{
    public bool DryRun { get; init; }

    public int? DefaultCompanyId { get; init; }

    public int TotalUnassignedAfter { get; init; }

    public bool HasUnassignedContent => TotalUnassignedAfter > 0;

    public int UpdatedRows { get; init; }
}
