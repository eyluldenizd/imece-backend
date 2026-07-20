using System.Text;

namespace Infrastructure.Database.Schema;

public sealed class SqlServerSchemaScriptGenerator : ISchemaScriptGenerator
{
    public string Generate(SchemaChange change) =>
        change.Kind switch
        {
            SchemaChangeKind.CreateTable when change.Table is not null =>
                GenerateCreateTable(change.Table),
            SchemaChangeKind.AddColumn when change.Column is not null =>
                GenerateAddColumn(change.TableName, change.Column),
            SchemaChangeKind.CreateIndex when change.Index is not null =>
                GenerateCreateIndex(change.TableName, change.Index),
            SchemaChangeKind.AddForeignKey when change.ForeignKey is not null =>
                GenerateAddForeignKey(change.TableName, change.ForeignKey),
            SchemaChangeKind.AddCheckConstraint when change.CheckConstraint is not null =>
                GenerateAddCheckConstraint(change.TableName, change.CheckConstraint),
            SchemaChangeKind.DropColumn when change.ObjectName is not null =>
                GenerateDropColumn(change.TableName, change.ObjectName),
            SchemaChangeKind.DropIndex when change.ObjectName is not null =>
                GenerateDropIndex(change.TableName, change.ObjectName),
            SchemaChangeKind.DropTable =>
                GenerateDropTable(change.TableName),
            _ => throw new InvalidOperationException(
                $"Schema change için script üretilemedi: {change.Kind} / {change.Description}")
        };

    public string GenerateCreateTable(TableDefinition table)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"IF OBJECT_ID(N'[dbo].[{table.Name}]', N'U') IS NULL");
        sb.AppendLine("BEGIN");
        sb.AppendLine($"    CREATE TABLE [dbo].[{table.Name}] (");

        var columnLines = new List<string>();
        foreach (var column in table.Columns)
        {
            var identity = column.IsIdentity ? " IDENTITY(1,1)" : string.Empty;
            var nullability = column.IsNullable ? " NULL" : " NOT NULL";
            var defaultExpr = !string.IsNullOrWhiteSpace(column.DefaultExpression)
                ? $" DEFAULT {column.DefaultExpression}"
                : string.Empty;
            columnLines.Add(
                $"        [{column.Name}] {column.SqlType}{identity}{nullability}{defaultExpr}");
        }

        var pkColumns = table.Columns.Where(c => c.IsPrimaryKey).Select(c => c.Name).ToList();
        if (pkColumns.Count > 0)
        {
            columnLines.Add(
                $"        CONSTRAINT [PK_{table.Name}] PRIMARY KEY CLUSTERED ({string.Join(", ", pkColumns.Select(c => $"[{c}]"))})");
        }

        foreach (var check in table.CheckConstraints)
        {
            columnLines.Add(
                $"        CONSTRAINT [{check.Name}] CHECK ({check.Expression})");
        }

        sb.AppendLine(string.Join(",\n", columnLines));
        sb.AppendLine("    );");

        // Indexes and FKs are applied as separate SchemaChange items so that
        // referenced tables can be created first in SafeApply mode.
        sb.AppendLine("END");
        return sb.ToString();
    }

    public string GenerateAddColumn(string tableName, ColumnDefinition column)
    {
        // Safe apply: NOT NULL without default cannot be added to non-empty tables.
        // Prefer nullable add; SchemaHardening can tighten later.
        var effectiveNullable = column.IsNullable
            || string.IsNullOrWhiteSpace(column.DefaultExpression);
        var nullability = effectiveNullable ? "NULL" : "NOT NULL";
        var defaultExpr = !string.IsNullOrWhiteSpace(column.DefaultExpression)
            ? $" DEFAULT {column.DefaultExpression}"
            : string.Empty;

        return $"""
            IF COL_LENGTH(N'dbo.{tableName}', N'{column.Name}') IS NULL
            BEGIN
                ALTER TABLE [dbo].[{tableName}]
                ADD [{column.Name}] {column.SqlType} {nullability}{defaultExpr};
            END
            """;
    }

    public string GenerateCreateIndex(string tableName, IndexDefinition index)
    {
        var unique = index.IsUnique ? "UNIQUE " : string.Empty;
        var columns = string.Join(", ", index.Columns.Select(c => $"[{c}]"));
        return $"""
            IF NOT EXISTS (
                SELECT 1 FROM sys.indexes
                WHERE name = N'{index.Name}'
                  AND object_id = OBJECT_ID(N'[dbo].[{tableName}]')
            )
            BEGIN
                CREATE {unique}NONCLUSTERED INDEX [{index.Name}]
                ON [dbo].[{tableName}] ({columns});
            END
            """;
    }

    public string GenerateAddForeignKey(string tableName, ForeignKeyDefinition foreignKey)
    {
        var columns = string.Join(", ", foreignKey.Columns.Select(c => $"[{c}]"));
        var refs = string.Join(", ", foreignKey.ReferencedColumns.Select(c => $"[{c}]"));
        return $"""
            IF OBJECT_ID(N'[dbo].[{foreignKey.Name}]', N'F') IS NULL
               AND OBJECT_ID(N'[dbo].[{tableName}]', N'U') IS NOT NULL
               AND OBJECT_ID(N'[dbo].[{foreignKey.ReferencedTable}]', N'U') IS NOT NULL
            BEGIN
                ALTER TABLE [dbo].[{tableName}]
                ADD CONSTRAINT [{foreignKey.Name}]
                FOREIGN KEY ({columns})
                REFERENCES [dbo].[{foreignKey.ReferencedTable}] ({refs})
                ON DELETE {foreignKey.OnDelete}
                ON UPDATE {foreignKey.OnUpdate};
            END
            """;
    }

    public string GenerateAddCheckConstraint(string tableName, CheckConstraintDefinition check)
    {
        return $"""
            IF OBJECT_ID(N'[dbo].[{check.Name}]', N'C') IS NULL
               AND OBJECT_ID(N'[dbo].[{tableName}]', N'U') IS NOT NULL
            BEGIN
                ALTER TABLE [dbo].[{tableName}]
                ADD CONSTRAINT [{check.Name}] CHECK ({check.Expression});
            END
            """;
    }

    public string GenerateDropColumn(string tableName, string columnName) =>
        $"""
        IF COL_LENGTH(N'dbo.{tableName}', N'{columnName}') IS NOT NULL
        BEGIN
            ALTER TABLE [dbo].[{tableName}] DROP COLUMN [{columnName}];
        END
        """;

    public string GenerateDropIndex(string tableName, string indexName) =>
        $"""
        IF EXISTS (
            SELECT 1 FROM sys.indexes
            WHERE name = N'{indexName}'
              AND object_id = OBJECT_ID(N'[dbo].[{tableName}]')
        )
        BEGIN
            DROP INDEX [{indexName}] ON [dbo].[{tableName}];
        END
        """;

    public string GenerateDropTable(string tableName) =>
        $"""
        IF OBJECT_ID(N'[dbo].[{tableName}]', N'U') IS NOT NULL
        BEGIN
            DROP TABLE [dbo].[{tableName}];
        END
        """;
}
