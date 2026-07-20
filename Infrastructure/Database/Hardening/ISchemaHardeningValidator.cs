namespace Infrastructure.Database.Hardening;

public interface ISchemaHardeningValidator
{
    Task<SchemaHardeningReport> RunAsync(CancellationToken cancellationToken = default);
}

public sealed class SchemaHardeningReport
{
    public bool ValidateOnly { get; init; }

    public bool ApplyRequested { get; init; }

    public int TotalViolations { get; init; }

    public bool HasViolations => TotalViolations > 0;

    public IReadOnlyList<string> Details { get; init; } = [];
}
