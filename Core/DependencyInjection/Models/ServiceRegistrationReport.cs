namespace Core.DependencyInjection.Models;

public sealed class ServiceRegistrationReport
{
    private readonly List<ServiceRegistrationEntry> _entries = [];

    public int Count => _entries.Count;

    public void Add(ServiceRegistrationEntry entry) => _entries.Add(entry);

    public string ToFormattedTable()
    {
        if (_entries.Count == 0)
        {
            return "(no convention registrations)";
        }

        return string.Join(
            Environment.NewLine,
            _entries.Select(e =>
                $"{e.ServiceType.PadRight(48)} -> {e.ImplementationType} ({e.Lifetime})"));
    }
}

public sealed class ServiceRegistrationEntry
{
    public required string ServiceType { get; init; }

    public required string ImplementationType { get; init; }

    public required string Lifetime { get; init; }
}

public sealed class ConventionRegistrationOptions
{
    public bool ThrowOnDuplicate { get; set; }

    public bool ThrowOnMultipleImplementations { get; set; }

    public bool EnableRegistrationReport { get; set; }
}
