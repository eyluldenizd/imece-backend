using Infrastructure.Database.Connections;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Database.Schema;

public sealed class SqlSchemaMetadataReader : ISchemaMetadataReader
{
    private readonly IDbExecutor _executor;

    public SqlSchemaMetadataReader(IDbExecutor executor)
    {
        _executor = executor;
    }

    public async Task<IReadOnlyDictionary<string, ExistingTableMetadata>> ReadAsync(
        SqlConnection connection,
        SqlTransaction? transaction = null,
        CancellationToken cancellationToken = default)
    {
        var tables = await _executor.QueryAsync(
            connection,
            """
            SELECT t.name AS TableName
            FROM sys.tables AS t
            INNER JOIN sys.schemas AS s ON s.schema_id = t.schema_id
            WHERE s.name = N'dbo'
              AND t.is_ms_shipped = 0
            ORDER BY t.name;
            """,
            reader => reader.GetString(0),
            transaction: transaction,
            cancellationToken: cancellationToken);

        var columns = await _executor.QueryAsync(
            connection,
            """
            SELECT
                t.name AS TableName,
                c.name AS ColumnName,
                ty.name AS TypeName,
                c.max_length,
                c.precision,
                c.scale,
                c.is_nullable,
                c.is_identity
            FROM sys.columns AS c
            INNER JOIN sys.tables AS t ON t.object_id = c.object_id
            INNER JOIN sys.schemas AS s ON s.schema_id = t.schema_id
            INNER JOIN sys.types AS ty ON ty.user_type_id = c.user_type_id
            WHERE s.name = N'dbo'
              AND t.is_ms_shipped = 0
            ORDER BY t.name, c.column_id;
            """,
            reader => new
            {
                TableName = reader.GetString(0),
                ColumnName = reader.GetString(1),
                TypeName = reader.GetString(2),
                MaxLength = reader.GetInt16(3),
                Precision = reader.GetByte(4),
                Scale = reader.GetByte(5),
                IsNullable = reader.GetBoolean(6),
                IsIdentity = reader.GetBoolean(7)
            },
            transaction: transaction,
            cancellationToken: cancellationToken);

        var indexes = await _executor.QueryAsync(
            connection,
            """
            SELECT t.name AS TableName, i.name AS IndexName
            FROM sys.indexes AS i
            INNER JOIN sys.tables AS t ON t.object_id = i.object_id
            INNER JOIN sys.schemas AS s ON s.schema_id = t.schema_id
            WHERE s.name = N'dbo'
              AND i.name IS NOT NULL
              AND i.is_primary_key = 0
              AND i.is_hypothetical = 0;
            """,
            reader => (Table: reader.GetString(0), Index: reader.GetString(1)),
            transaction: transaction,
            cancellationToken: cancellationToken);

        var foreignKeys = await _executor.QueryAsync(
            connection,
            """
            SELECT t.name AS TableName, fk.name AS FkName
            FROM sys.foreign_keys AS fk
            INNER JOIN sys.tables AS t ON t.object_id = fk.parent_object_id
            INNER JOIN sys.schemas AS s ON s.schema_id = t.schema_id
            WHERE s.name = N'dbo';
            """,
            reader => (Table: reader.GetString(0), Name: reader.GetString(1)),
            transaction: transaction,
            cancellationToken: cancellationToken);

        var checks = await _executor.QueryAsync(
            connection,
            """
            SELECT t.name AS TableName, cc.name AS CheckName
            FROM sys.check_constraints AS cc
            INNER JOIN sys.tables AS t ON t.object_id = cc.parent_object_id
            INNER JOIN sys.schemas AS s ON s.schema_id = t.schema_id
            WHERE s.name = N'dbo';
            """,
            reader => (Table: reader.GetString(0), Name: reader.GetString(1)),
            transaction: transaction,
            cancellationToken: cancellationToken);

        var result = new Dictionary<string, ExistingTableMetadata>(StringComparer.OrdinalIgnoreCase);

        foreach (var tableName in tables)
        {
            result[tableName] = new ExistingTableMetadata
            {
                Name = tableName,
                Columns = new Dictionary<string, ExistingColumnMetadata>(StringComparer.OrdinalIgnoreCase),
                Indexes = new HashSet<string>(StringComparer.OrdinalIgnoreCase),
                ForeignKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase),
                CheckConstraints = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            };
        }

        foreach (var column in columns)
        {
            if (!result.TryGetValue(column.TableName, out var table))
            {
                continue;
            }

            var sqlType = FormatSqlType(column.TypeName, column.MaxLength, column.Precision, column.Scale);
            var mutableColumns = (Dictionary<string, ExistingColumnMetadata>)table.Columns;
            mutableColumns[column.ColumnName] = new ExistingColumnMetadata
            {
                Name = column.ColumnName,
                SqlType = sqlType,
                IsNullable = column.IsNullable,
                IsIdentity = column.IsIdentity
            };
        }

        foreach (var index in indexes)
        {
            if (result.TryGetValue(index.Table, out var table))
            {
                ((HashSet<string>)table.Indexes).Add(index.Index);
            }
        }

        foreach (var fk in foreignKeys)
        {
            if (result.TryGetValue(fk.Table, out var table))
            {
                ((HashSet<string>)table.ForeignKeys).Add(fk.Name);
            }
        }

        foreach (var check in checks)
        {
            if (result.TryGetValue(check.Table, out var table))
            {
                ((HashSet<string>)table.CheckConstraints).Add(check.Name);
            }
        }

        return result;
    }

    private static string FormatSqlType(string typeName, short maxLength, byte precision, byte scale)
    {
        return typeName.ToLowerInvariant() switch
        {
            "nvarchar" or "varchar" or "nchar" or "char" =>
                maxLength == -1
                    ? $"{typeName.ToUpperInvariant()}(MAX)"
                    : $"{typeName.ToUpperInvariant()}({(typeName.StartsWith("n", StringComparison.OrdinalIgnoreCase) ? maxLength / 2 : maxLength)})",
            "decimal" or "numeric" => $"{typeName.ToUpperInvariant()}({precision},{scale})",
            "varbinary" or "binary" =>
                maxLength == -1
                    ? $"{typeName.ToUpperInvariant()}(MAX)"
                    : $"{typeName.ToUpperInvariant()}({maxLength})",
            _ => typeName.ToUpperInvariant()
        };
    }
}
