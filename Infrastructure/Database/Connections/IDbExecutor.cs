using System.Data;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Database.Connections;

public interface IDbExecutor
{
    Task<int> ExecuteNonQueryAsync(
        SqlConnection connection,
        string sql,
        IEnumerable<SqlParameter>? parameters = null,
        SqlTransaction? transaction = null,
        int? commandTimeoutSeconds = null,
        CancellationToken cancellationToken = default);

    Task<object?> ExecuteScalarAsync(
        SqlConnection connection,
        string sql,
        IEnumerable<SqlParameter>? parameters = null,
        SqlTransaction? transaction = null,
        int? commandTimeoutSeconds = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<T>> QueryAsync<T>(
        SqlConnection connection,
        string sql,
        Func<SqlDataReader, T> map,
        IEnumerable<SqlParameter>? parameters = null,
        SqlTransaction? transaction = null,
        int? commandTimeoutSeconds = null,
        CancellationToken cancellationToken = default);
}
