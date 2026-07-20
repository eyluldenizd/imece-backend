namespace Infrastructure.Database.Schema;

public sealed class SchemaDiffer : ISchemaDiffer
{
    public SchemaDiff Diff(
        IReadOnlyList<TableDefinition> expectedTables,
        IReadOnlyDictionary<string, ExistingTableMetadata> existingTables)
    {
        var changes = new List<SchemaChange>();

        foreach (var expected in expectedTables)
        {
            ExistingTableMetadata? existing = null;
            var tableExists = existingTables.TryGetValue(expected.Name, out existing);

            if (!tableExists)
            {
                changes.Add(new SchemaChange
                {
                    Kind = SchemaChangeKind.CreateTable,
                    TableName = expected.Name,
                    ObjectName = expected.Name,
                    Table = expected,
                    Description = $"CREATE TABLE {expected.Name}"
                });
            }
            else
            {
                foreach (var column in expected.Columns)
                {
                    if (!existing!.Columns.ContainsKey(column.Name))
                    {
                        changes.Add(new SchemaChange
                        {
                            Kind = SchemaChangeKind.AddColumn,
                            TableName = expected.Name,
                            ObjectName = column.Name,
                            Column = column,
                            Description = $"ADD COLUMN {expected.Name}.{column.Name}"
                        });
                    }
                }
            }

            foreach (var index in expected.Indexes)
            {
                if (tableExists && existing!.Indexes.Contains(index.Name))
                {
                    continue;
                }

                changes.Add(new SchemaChange
                {
                    Kind = SchemaChangeKind.CreateIndex,
                    TableName = expected.Name,
                    ObjectName = index.Name,
                    Index = index,
                    Description = $"CREATE INDEX {index.Name} ON {expected.Name}"
                });
            }

            foreach (var fk in expected.ForeignKeys)
            {
                if (tableExists && existing!.ForeignKeys.Contains(fk.Name))
                {
                    continue;
                }

                changes.Add(new SchemaChange
                {
                    Kind = SchemaChangeKind.AddForeignKey,
                    TableName = expected.Name,
                    ObjectName = fk.Name,
                    ForeignKey = fk,
                    Description = $"ADD FK {fk.Name} ON {expected.Name}"
                });
            }

            foreach (var check in expected.CheckConstraints)
            {
                if (tableExists && existing!.CheckConstraints.Contains(check.Name))
                {
                    continue;
                }

                changes.Add(new SchemaChange
                {
                    Kind = SchemaChangeKind.AddCheckConstraint,
                    TableName = expected.Name,
                    ObjectName = check.Name,
                    CheckConstraint = check,
                    Description = $"ADD CHECK {check.Name} ON {expected.Name}"
                });
            }
        }

        return new SchemaDiff { Changes = changes };
    }
}
