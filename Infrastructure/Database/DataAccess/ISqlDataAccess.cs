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

    /// <summary>
    /// Opens one connection and runs the work inside a SQL transaction.
    /// Commit on success; rollback on exception.
    /// </summary>
    Task ExecuteInTransactionAsync(
        Func<SqlConnection, SqlTransaction, CancellationToken, Task> work,
        CancellationToken cancellationToken = default);

    Task<T> ExecuteInTransactionAsync<T>(
        Func<SqlConnection, SqlTransaction, CancellationToken, Task<T>> work,
        CancellationToken cancellationToken = default);
}
