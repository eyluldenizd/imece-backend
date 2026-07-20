namespace Infrastructure.Database.Seeding;

public enum DevelopmentDatabaseResetOutcome
{
    NotRequested = 0,
    NotAllowed = 1,
    SkippedAlreadyApplied = 2,
    Wiped = 3
}

public sealed class DevelopmentDatabaseResetResult
{
    public DevelopmentDatabaseResetOutcome Outcome { get; init; }

    public string? Detail { get; init; }
}

public interface IDevelopmentDatabaseResetter
{
    Task<DevelopmentDatabaseResetResult> TryResetAsync(CancellationToken cancellationToken = default);
}
