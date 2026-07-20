namespace Infrastructure.Database.Options;

public sealed class ContentScopeBackfillOptions
{
    public const string SectionName = "ContentScopeBackfill";

    public bool Enabled { get; set; }

    public string? DefaultCompanyCode { get; set; }

    public bool FailOnUnassignedContent { get; set; } = true;

    public bool DryRun { get; set; } = true;

    public int BatchSize { get; set; } = 500;
}
