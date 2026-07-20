namespace Infrastructure.Database.Options;

public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    public string Profile { get; set; } = "LocalIntegrated";

    public string Server { get; set; } = "localhost";

    public string? Instance { get; set; }

    public string DatabaseName { get; set; } = "SankoImece";

    public bool CreateIfMissing { get; set; }

    public bool Encrypt { get; set; } = true;

    public bool TrustServerCertificate { get; set; }

    public int ConnectTimeoutSeconds { get; set; } = 30;

    public int CommandTimeoutSeconds { get; set; } = 60;

    public bool IntegratedSecurity { get; set; } = true;

    public string? UserName { get; set; }

    public string? Password { get; set; }
}
