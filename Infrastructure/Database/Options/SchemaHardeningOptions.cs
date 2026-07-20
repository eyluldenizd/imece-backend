namespace Infrastructure.Database.Options;

public sealed class SchemaHardeningOptions
{
    public const string SectionName = "SchemaHardening";

    public bool Enabled { get; set; }

    public bool ValidateOnly { get; set; } = true;

    public bool Apply { get; set; }

    public bool FailOnViolation { get; set; }
}
