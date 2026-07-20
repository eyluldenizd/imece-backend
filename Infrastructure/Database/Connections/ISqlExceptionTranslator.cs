using Microsoft.Data.SqlClient;

namespace Infrastructure.Database.Connections;

public interface ISqlExceptionTranslator
{
    Exception Translate(SqlException exception, string? operation = null);
}
