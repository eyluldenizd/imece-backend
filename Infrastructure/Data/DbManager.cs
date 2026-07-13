using System.Data;
using System.Reflection;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Data;

public sealed class DbManager
{
    private readonly string _connectionString;

    public DbManager(IConfiguration configuration)
    {
        _connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "DefaultConnection bağlantı bilgisi bulunamadı.");
    }

    private SqlConnection CreateConnection() => new(_connectionString);

    /// <summary>
    /// Property'nin veritabanındaki kolon adı farklıysa bu attribute ile belirtilebilir.
    /// Belirtilmezse property adı kolon adı olarak kabul edilir.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class DbColumnAttribute : Attribute
    {
        public string ColumnName { get; }
        public DbColumnAttribute(string columnName) => ColumnName = columnName;
    }

    public async Task<List<T>> QueryAsync<T>(
        string sql,
        IEnumerable<SqlParameter>? parameters = null,
        CancellationToken cancellationToken = default) where T : new()
    {
        var columnMappings = GetColumnMappings<T>();
        var results = new List<T>();

        await using var connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);
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

        await using var connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);
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
        await using var connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(sql, connection);
        AddParameters(command, parameters);
        return await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<T?> ExecuteScalarAsync<T>(
        string sql,
        IEnumerable<SqlParameter>? parameters = null,
        CancellationToken cancellationToken = default)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(sql, connection);
        AddParameters(command, parameters);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        if (result is null || result == DBNull.Value)
        {
            return default;
        }

        return (T)Convert.ChangeType(result, typeof(T));
    }

    /// <summary>
    /// Stored procedure çalıştırır. Örnekteki RunProcedureAsync karşılığıdır.
    /// </summary>
    public async Task<(bool IsSuccess, string? ErrorMessage)> ExecuteProcedureAsync(
        string procedureName,
        IEnumerable<SqlParameter>? parameters = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync(cancellationToken);
            await using var command = new SqlCommand(procedureName, connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            AddParameters(command, parameters);
            await command.ExecuteNonQueryAsync(cancellationToken);
            return (true, null);
        }
        catch (SqlException ex)
        {
            return (false, ex.Message);
        }
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
            var columnAttribute = property.GetCustomAttribute<DbColumnAttribute>();
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

            var convertedValue = targetType.IsEnum
                ? Enum.ToObject(targetType, value)
                : Convert.ChangeType(value, targetType);

            property.SetValue(model, convertedValue);
        }

        return model;
    }
}