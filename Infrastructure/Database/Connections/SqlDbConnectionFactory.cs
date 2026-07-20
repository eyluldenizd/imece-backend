using Microsoft.Data.SqlClient;

namespace Infrastructure.Database.Connections;

public sealed class SqlDbConnectionFactory : IDbConnectionFactory
{
    private readonly IConnectionStringFactory _connectionStringFactory;

    public SqlDbConnectionFactory(IConnectionStringFactory connectionStringFactory)
    {
        _connectionStringFactory = connectionStringFactory;
    }

    public async Task<SqlConnection> OpenApplicationConnectionAsync(
        CancellationToken cancellationToken = default)
    {
        var connection = new SqlConnection(_connectionStringFactory.GetApplicationConnectionString());
        await connection.OpenAsync(cancellationToken);
        return connection;
    }

    public async Task<SqlConnection> OpenMasterConnectionAsync(
        CancellationToken cancellationToken = default)
    {
        var connection = new SqlConnection(_connectionStringFactory.GetMasterConnectionString());
        await connection.OpenAsync(cancellationToken);
        return connection;
    }
}
