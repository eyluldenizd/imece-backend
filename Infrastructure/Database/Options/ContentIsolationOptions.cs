namespace Infrastructure.Database.Options;

public sealed class ContentIsolationOptions
{
    public const string SectionName = "ContentIsolation";

    public bool Enforce { get; set; }
}
