namespace Infrastructure.Database.Schema;

public sealed class ColumnDefinition
{
    public required string Name { get; init; }

    public required string SqlType { get; init; }

    public bool IsNullable { get; init; }

    public bool IsIdentity { get; init; }

    public bool IsPrimaryKey { get; init; }

    public string? DefaultExpression { get; init; }
}

public sealed class IndexDefinition
{
    public required string Name { get; init; }

    public required IReadOnlyList<string> Columns { get; init; }

    public bool IsUnique { get; init; }

    /// <summary>Optional SQL Server filtered index predicate (without WHERE keyword).</summary>
    public string? FilterExpression { get; init; }
}

public sealed class ForeignKeyDefinition
{
    public required string Name { get; init; }

    public required IReadOnlyList<string> Columns { get; init; }

    public required string ReferencedTable { get; init; }

    public required IReadOnlyList<string> ReferencedColumns { get; init; }

    public string OnDelete { get; init; } = "NO ACTION";

    public string OnUpdate { get; init; } = "NO ACTION";
}

public sealed class CheckConstraintDefinition
{
    public required string Name { get; init; }

    public required string Expression { get; init; }

    /// <summary>Optional SQL run before ADD CONSTRAINT (e.g. data backfill).</summary>
    public string? PreApplySql { get; init; }
}

public sealed class DefaultConstraintDefinition
{
    public required string Name { get; init; }

    public required string ColumnName { get; init; }

    public required string Expression { get; init; }
}

public sealed class TableDefinition
{
    public required string Name { get; init; }

    public string Schema { get; init; } = "dbo";

    public required IReadOnlyList<ColumnDefinition> Columns { get; init; }

    public IReadOnlyList<IndexDefinition> Indexes { get; init; } = [];

    public IReadOnlyList<ForeignKeyDefinition> ForeignKeys { get; init; } = [];

    public IReadOnlyList<CheckConstraintDefinition> CheckConstraints { get; init; } = [];

    public IReadOnlyList<DefaultConstraintDefinition> DefaultConstraints { get; init; } = [];
}

public interface ISchemaDefinition
{
    string FeatureName { get; }

    IReadOnlyList<TableDefinition> Tables { get; }
}
