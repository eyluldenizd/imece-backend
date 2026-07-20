namespace Infrastructure.Database.Schema;

public interface ISchemaScriptGenerator
{
    string GenerateCreateTable(TableDefinition table);

    string GenerateAddColumn(string tableName, ColumnDefinition column);

    string GenerateCreateIndex(string tableName, IndexDefinition index);

    string GenerateAddForeignKey(string tableName, ForeignKeyDefinition foreignKey);

    string GenerateAddCheckConstraint(string tableName, CheckConstraintDefinition check);

    string GenerateDropColumn(string tableName, string columnName);

    string GenerateDropIndex(string tableName, string indexName);

    string GenerateDropTable(string tableName);

    string Generate(SchemaChange change);
}
