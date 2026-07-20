using Microsoft.Data.SqlClient;

namespace Infrastructure.Database.Seeding;

public interface IDevelopmentDataSeeder
{
    Task SeedAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        int commandTimeoutSeconds,
        CancellationToken cancellationToken = default);
}
