using Infrastructure.Database.Connections;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Database.Seeding;

public interface IRealisticDevelopmentContentSeeder
{
    Task SeedAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        int commandTimeoutSeconds,
        string seedVersion,
        CancellationToken cancellationToken = default);
}
