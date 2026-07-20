using Microsoft.Data.SqlClient;

namespace Infrastructure.Database.Connections;

public sealed class SqlDbExecutor : IDbExecutor
{
    public async Task<int> ExecuteNonQueryAsync(
        SqlConnection connection,
        string sql,
        IEnumerable<SqlParameter>? parameters = null,
        SqlTransaction? transaction = null,
        int? commandTimeoutSeconds = null,
        CancellationToken cancellationToken = default)
    {
        await using var command = CreateCommand(
            connection, sql, parameters, transaction, commandTimeoutSeconds);
        return await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<object?> ExecuteScalarAsync(
        SqlConnection connection,
        string sql,
        IEnumerable<SqlParameter>? parameters = null,
        SqlTransaction? transaction = null,
        int? commandTimeoutSeconds = null,
        CancellationToken cancellationToken = default)
    {
        await using var command = CreateCommand(
            connection, sql, parameters, transaction, commandTimeoutSeconds);
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is DBNull ? null : result;
    }

    public async Task<IReadOnlyList<T>> QueryAsync<T>(
        SqlConnection connection,
        string sql,
        Func<SqlDataReader, T> map,
        IEnumerable<SqlParameter>? parameters = null,
        SqlTransaction? transaction = null,
        int? commandTimeoutSeconds = null,
        CancellationToken cancellationToken = default)
    {
        await using var command = CreateCommand(
            connection, sql, parameters, transaction, commandTimeoutSeconds);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var results = new List<T>();
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(map(reader));
        }

        return results;
    }

    private static SqlCommand CreateCommand(
        SqlConnection connection,
        string sql,
        IEnumerable<SqlParameter>? parameters,
        SqlTransaction? transaction,
        int? commandTimeoutSeconds)
    {
        var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Transaction = transaction;

        if (commandTimeoutSeconds is > 0)
        {
            command.CommandTimeout = commandTimeoutSeconds.Value;
        }

        if (parameters is not null)
        {
            foreach (var parameter in parameters)
            {
                command.Parameters.Add(parameter);
            }
        }

        return command;
    }
}
