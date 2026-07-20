using System.Collections;
using System.Reflection;
using System.Text.Json;
using Core.Auditing;

namespace Infrastructure.Database.Audit;

public sealed class AuditValueSanitizer : IAuditValueSanitizer
{
    private static readonly HashSet<string> SensitiveNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "password", "passwd", "secret", "token", "accesstoken", "refreshtoken",
        "apikey", "api_key", "authorization", "connectionstring", "credential"
    };

    public object? Sanitize(object? value)
    {
        if (value is null)
        {
            return null;
        }

        if (value is string or ValueType)
        {
            return value;
        }

        if (value is IDictionary dictionary)
        {
            var result = new Dictionary<string, object?>();
            foreach (DictionaryEntry entry in dictionary)
            {
                var key = entry.Key?.ToString() ?? string.Empty;
                result[key] = IsSensitive(key) ? "***" : Sanitize(entry.Value);
            }

            return result;
        }

        if (value is IEnumerable enumerable and not string)
        {
            return enumerable.Cast<object?>().Select(Sanitize).ToList();
        }

        var type = value.GetType();
        if (type.Namespace?.StartsWith("System", StringComparison.Ordinal) == true)
        {
            return value;
        }

        var bag = new Dictionary<string, object?>();
        foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            if (!property.CanRead || property.GetIndexParameters().Length > 0)
            {
                continue;
            }

            var propValue = property.GetValue(value);
            bag[property.Name] = IsSensitive(property.Name) ? "***" : Sanitize(propValue);
        }

        return bag;
    }

    public string? ToJson(object? value)
    {
        var sanitized = Sanitize(value);
        if (sanitized is null)
        {
            return null;
        }

        return JsonSerializer.Serialize(sanitized);
    }

    private static bool IsSensitive(string name)
    {
        var compact = name.Replace("_", string.Empty, StringComparison.Ordinal)
            .Replace("-", string.Empty, StringComparison.Ordinal);
        return SensitiveNames.Contains(compact)
            || SensitiveNames.Any(s => compact.Contains(s, StringComparison.OrdinalIgnoreCase));
    }
}
