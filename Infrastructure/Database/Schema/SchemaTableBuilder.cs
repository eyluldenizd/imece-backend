namespace Infrastructure.Database.Schema;

internal static class SchemaTableBuilder
{
    public static ColumnDefinition Col(
        string name,
        string sqlType,
        bool nullable = false,
        bool identity = false,
        bool primaryKey = false,
        string? defaultExpression = null) =>
        new()
        {
            Name = name,
            SqlType = sqlType,
            IsNullable = nullable,
            IsIdentity = identity,
            IsPrimaryKey = primaryKey,
            DefaultExpression = defaultExpression
        };

    public static IndexDefinition Idx(
        string name,
        bool unique,
        params string[] columns) =>
        new()
        {
            Name = name,
            IsUnique = unique,
            Columns = columns
        };

    public static ForeignKeyDefinition Fk(
        string name,
        string column,
        string referencedTable,
        string referencedColumn,
        string onDelete = "NO ACTION") =>
        new()
        {
            Name = name,
            Columns = [column],
            ReferencedTable = referencedTable,
            ReferencedColumns = [referencedColumn],
            OnDelete = onDelete
        };

    public static TableDefinition Table(
        string name,
        IReadOnlyList<ColumnDefinition> columns,
        IReadOnlyList<IndexDefinition>? indexes = null,
        IReadOnlyList<ForeignKeyDefinition>? foreignKeys = null,
        IReadOnlyList<CheckConstraintDefinition>? checks = null) =>
        new()
        {
            Name = name,
            Columns = columns,
            Indexes = indexes ?? [],
            ForeignKeys = foreignKeys ?? [],
            CheckConstraints = checks ?? []
        };
}
