namespace Infrastructure.Database.Options;

public enum AuditContentFailureMode
{
    FailOpen = 0,
    FailClosed = 1
}

public sealed class AuditOptions
{
    public const string SectionName = "Audit";

    /// <summary>Ana anahtar. Kapalıysa tüm otomatik kancalar no-op.</summary>
    public bool Enabled { get; set; }

    public AuditContentFailureMode ContentFailureMode { get; set; } =
        AuditContentFailureMode.FailOpen;

    public bool CaptureHttpRequests { get; set; } = true;

    public bool CaptureMutations { get; set; } = true;

    public bool CaptureErrors { get; set; } = true;

    public bool CaptureSqlWrites { get; set; } = false;

    /// <summary>Başarısız ServiceResult (BadRequest/NotFound/Conflict) mutation audit.</summary>
    public bool CaptureFailedResults { get; set; } = true;

    /// <summary>true: Channel + BackgroundService; false: istek içinde yaz.</summary>
    public bool UseBackgroundQueue { get; set; } = true;

    public int MaxBodyBytes { get; set; } = 8_192;

    public int QueueCapacity { get; set; } = 2_048;

    public string[] ExcludedPathPrefixes { get; set; } =
    [
        "/health",
        "/swagger"
    ];
}
