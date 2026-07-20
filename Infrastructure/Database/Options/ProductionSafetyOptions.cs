namespace Infrastructure.Database.Options;

public sealed class ProductionSafetyOptions
{
    public const string SectionName = "ProductionSafety";

    public bool FailStartupOnUnsafeConfiguration { get; set; }
}
