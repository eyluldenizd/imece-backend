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

    public static IndexDefinition IdxFiltered(
        string name,
        bool unique,
        string filterExpression,
        params string[] columns) =>
        new()
        {
            Name = name,
            IsUnique = unique,
            Columns = columns,
            FilterExpression = filterExpression
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

    public static ColumnDefinition[] OrganizationScopeColumns() =>
    [
        Col("company_scope", "NVARCHAR(16)", defaultExpression: "N'All'"),
        Col("company_id", "INT", nullable: true),
        Col("branch_scope", "NVARCHAR(16)", defaultExpression: "N'All'"),
        Col("branch_id", "INT", nullable: true),
        Col("department_scope", "NVARCHAR(16)", defaultExpression: "N'All'"),
        Col("department_id", "INT", nullable: true)
    ];

    public static ForeignKeyDefinition[] OrganizationScopeForeignKeys(string tableName) =>
    [
        Fk($"FK_{tableName}_companies", "company_id", "companies", "company_id"),
        Fk($"FK_{tableName}_branches", "branch_id", "branches", "branch_id"),
        Fk($"FK_{tableName}_departments", "department_id", "departments", "department_id")
    ];
}
