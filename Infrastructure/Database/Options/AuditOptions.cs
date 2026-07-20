namespace Infrastructure.Database.Options;

public enum AuditContentFailureMode
{
    FailOpen = 0,
    FailClosed = 1
}

public sealed class AuditOptions
{
    public const string SectionName = "Audit";

    public bool Enabled { get; set; }

    public AuditContentFailureMode ContentFailureMode { get; set; } = AuditContentFailureMode.FailOpen;
}
