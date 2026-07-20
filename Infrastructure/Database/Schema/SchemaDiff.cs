namespace Infrastructure.Database.Schema;

public enum SchemaChangeKind
{
    CreateTable = 0,
    AddColumn = 1,
    CreateIndex = 2,
    AddForeignKey = 3,
    AddCheckConstraint = 4,
    DropColumn = 5,
    DropIndex = 6,
    DropTable = 7,
    AlterColumn = 8
}

public sealed class SchemaChange
{
    public required SchemaChangeKind Kind { get; init; }

    public required string TableName { get; init; }

    public string? ObjectName { get; init; }

    public bool IsDestructive { get; init; }

    public required string Description { get; init; }

    public ColumnDefinition? Column { get; init; }

    public IndexDefinition? Index { get; init; }

    public ForeignKeyDefinition? ForeignKey { get; init; }

    public CheckConstraintDefinition? CheckConstraint { get; init; }

    public TableDefinition? Table { get; init; }
}

public sealed class SchemaDiff
{
    public IReadOnlyList<SchemaChange> Changes { get; init; } = [];

    public bool HasChanges => Changes.Count > 0;

    public bool HasDestructiveChanges => Changes.Any(c => c.IsDestructive);
}

public sealed class SchemaSyncResult
{
    public bool Succeeded { get; init; }

    public string? ErrorMessage { get; init; }

    public int AppliedChangeCount { get; init; }

    public int PendingChangeCount { get; init; }

    public static SchemaSyncResult Success(int applied, int pending = 0) =>
        new() { Succeeded = true, AppliedChangeCount = applied, PendingChangeCount = pending };

    public static SchemaSyncResult Failure(string message) =>
        new() { Succeeded = false, ErrorMessage = message };
}
