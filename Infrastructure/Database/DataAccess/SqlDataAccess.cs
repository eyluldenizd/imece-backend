using System.Reflection;
using Infrastructure.Data;
using Infrastructure.Database.Connections;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Database.DataAccess;

/// <summary>
/// DbManager yerine kullanılan SQL veri erişim katmanı. Kolon eşlemesi
/// <see cref="DbManager.DbColumnAttribute"/> ile uyumludur.
/// </summary>
public sealed class SqlDataAccess : ISqlDataAccess
{
    private readonly IDbConnectionFactory _connectionFactory;

    public SqlDataAccess(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<List<T>> QueryAsync<T>(
        string sql,
        IEnumerable<SqlParameter>? parameters = null,
        CancellationToken cancellationToken = default) where T : new()
    {
        var columnMappings = GetColumnMappings<T>();
        var results = new List<T>();

        await using var connection = await _connectionFactory.OpenApplicationConnectionAsync(cancellationToken);
        await using var command = new SqlCommand(sql, connection);
        AddParameters(command, parameters);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(MapRow<T>(reader, columnMappings));
        }

        return results;
    }

    public async Task<T?> QueryFirstOrDefaultAsync<T>(
        string sql,
        IEnumerable<SqlParameter>? parameters = null,
        CancellationToken cancellationToken = default) where T : new()
    {
        var columnMappings = GetColumnMappings<T>();

        await using var connection = await _connectionFactory.OpenApplicationConnectionAsync(cancellationToken);
        await using var command = new SqlCommand(sql, connection);
        AddParameters(command, parameters);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
        {
            return default;
        }

        return MapRow<T>(reader, columnMappings);
    }

    public async Task<int> ExecuteAsync(
        string sql,
        IEnumerable<SqlParameter>? parameters = null,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.OpenApplicationConnectionAsync(cancellationToken);
        await using var command = new SqlCommand(sql, connection);
        AddParameters(command, parameters);
        return await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<T?> ExecuteScalarAsync<T>(
        string sql,
        IEnumerable<SqlParameter>? parameters = null,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.OpenApplicationConnectionAsync(cancellationToken);
        await using var command = new SqlCommand(sql, connection);
        AddParameters(command, parameters);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        if (result is null || result == DBNull.Value)
        {
            return default;
        }

        var targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
        return (T)Convert.ChangeType(result, targetType);
    }

    private static void AddParameters(
        SqlCommand command,
        IEnumerable<SqlParameter>? parameters)
    {
        if (parameters is null)
        {
            return;
        }

        command.Parameters.AddRange(parameters.ToArray());
    }

    private static Dictionary<string, PropertyInfo> GetColumnMappings<T>() where T : new()
    {
        var mappings = new Dictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);

        foreach (var property in typeof(T).GetProperties())
        {
            var columnAttribute = property.GetCustomAttribute<DbManager.DbColumnAttribute>();
            var columnName = columnAttribute?.ColumnName ?? property.Name;
            mappings[columnName] = property;
        }

        return mappings;
    }

    private static T MapRow<T>(
        SqlDataReader reader,
        Dictionary<string, PropertyInfo> columnMappings) where T : new()
    {
        var model = new T();

        for (var i = 0; i < reader.FieldCount; i++)
        {
            var columnName = reader.GetName(i);
            if (!columnMappings.TryGetValue(columnName, out var property))
            {
                continue;
            }

            if (reader.IsDBNull(i))
            {
                continue;
            }

            var value = reader.GetValue(i);
            var targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

            object? convertedValue;
            if (targetType.IsEnum)
            {
                convertedValue = Enum.ToObject(targetType, value);
            }
            else if (targetType == typeof(DateOnly) && value is DateTime dateTime)
            {
                convertedValue = DateOnly.FromDateTime(dateTime);
            }
            else if (targetType == typeof(TimeOnly) && value is TimeSpan timeSpan)
            {
                convertedValue = TimeOnly.FromTimeSpan(timeSpan);
            }
            else
            {
                convertedValue = Convert.ChangeType(value, targetType);
            }

            property.SetValue(model, convertedValue);
        }

        return model;
    }
}
