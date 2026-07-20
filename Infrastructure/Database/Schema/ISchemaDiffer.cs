namespace Infrastructure.Database.Schema;

public interface ISchemaDiffer
{
    SchemaDiff Diff(
        IReadOnlyList<TableDefinition> expectedTables,
        IReadOnlyDictionary<string, ExistingTableMetadata> existingTables);
}

public sealed class ExistingTableMetadata
{
    public required string Name { get; init; }

    public IReadOnlyDictionary<string, ExistingColumnMetadata> Columns { get; init; }
        = new Dictionary<string, ExistingColumnMetadata>(StringComparer.OrdinalIgnoreCase);

    public IReadOnlySet<string> Indexes { get; init; }
        = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    public IReadOnlySet<string> ForeignKeys { get; init; }
        = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    public IReadOnlySet<string> CheckConstraints { get; init; }
        = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
}

public sealed class ExistingColumnMetadata
{
    public required string Name { get; init; }

    public required string SqlType { get; init; }

    public bool IsNullable { get; init; }

    public bool IsIdentity { get; init; }
}
