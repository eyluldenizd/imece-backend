using Microsoft.Data.SqlClient;

namespace Infrastructure.Database.DataAccess;

public interface ISqlDataAccess
{
    Task<List<T>> QueryAsync<T>(
        string sql,
        IEnumerable<SqlParameter>? parameters = null,
        CancellationToken cancellationToken = default) where T : new();

    Task<T?> QueryFirstOrDefaultAsync<T>(
        string sql,
        IEnumerable<SqlParameter>? parameters = null,
        CancellationToken cancellationToken = default) where T : new();

    Task<int> ExecuteAsync(
        string sql,
        IEnumerable<SqlParameter>? parameters = null,
        CancellationToken cancellationToken = default);

    Task<T?> ExecuteScalarAsync<T>(
        string sql,
        IEnumerable<SqlParameter>? parameters = null,
        CancellationToken cancellationToken = default);
}
