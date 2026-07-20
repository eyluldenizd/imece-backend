using Microsoft.Data.SqlClient;

namespace Infrastructure.Database.Connections;

public interface IDbConnectionFactory
{
    Task<SqlConnection> OpenApplicationConnectionAsync(CancellationToken cancellationToken = default);

    Task<SqlConnection> OpenMasterConnectionAsync(CancellationToken cancellationToken = default);
}
