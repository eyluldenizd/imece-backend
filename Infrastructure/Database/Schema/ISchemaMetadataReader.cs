using Microsoft.Data.SqlClient;

namespace Infrastructure.Database.Schema;

public interface ISchemaMetadataReader
{
    Task<IReadOnlyDictionary<string, ExistingTableMetadata>> ReadAsync(
        SqlConnection connection,
        SqlTransaction? transaction = null,
        CancellationToken cancellationToken = default);
}
