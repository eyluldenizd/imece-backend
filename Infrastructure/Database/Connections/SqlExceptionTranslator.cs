using Microsoft.Data.SqlClient;

namespace Infrastructure.Database.Connections;

public sealed class SqlExceptionTranslator : ISqlExceptionTranslator
{
    public Exception Translate(SqlException exception, string? operation = null)
    {
        var prefix = string.IsNullOrWhiteSpace(operation)
            ? "SQL işlemi başarısız"
            : $"SQL işlemi başarısız ({operation})";

        return exception.Number switch
        {
            2 or 53 or -2 => new TimeoutException(
                $"{prefix}: bağlantı/zaman aşımı (SqlError={exception.Number}).", exception),
            18456 => new UnauthorizedAccessException(
                $"{prefix}: kimlik doğrulama hatası.", exception),
            208 => new InvalidOperationException(
                $"{prefix}: nesne bulunamadı (geçersiz nesne adı).", exception),
            547 => new InvalidOperationException(
                $"{prefix}: kısıt ihlali (FK/CHECK).", exception),
            2627 or 2601 => new InvalidOperationException(
                $"{prefix}: benzersizlik ihlali.", exception),
            _ => new InvalidOperationException(
                $"{prefix}: {exception.Message}", exception)
        };
    }
}
