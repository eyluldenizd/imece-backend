namespace Infrastructure.Database.Options;

public enum SchemaSyncMode
{
    ValidateOnly = 0,
    SafeApply = 1,
    DestructiveApply = 2
}

public sealed class DatabaseSchemaOptions
{
    public const string SectionName = "DatabaseSchema";

    public bool Enabled { get; set; }

    public SchemaSyncMode Mode { get; set; } = SchemaSyncMode.ValidateOnly;

    public bool CreateDatabaseIfMissing { get; set; }

    public bool AllowDestructiveChanges { get; set; }

    public bool FailApplicationOnError { get; set; } = true;

    public int CommandTimeoutSeconds { get; set; } = 120;

    public int LockTimeoutSeconds { get; set; } = 60;

    public bool LogGeneratedSql { get; set; }

    public bool SeedDevelopmentData { get; set; }

    public bool ResetDatabase { get; set; }

    public string[] AllowedResetDatabaseNames { get; set; } =
        ["SankoImece", "SankoImece_Test"];

    public string DevelopmentSeedVersion { get; set; } = "realistic-v1";

    public bool SeedRealisticContent { get; set; } = true;
}
