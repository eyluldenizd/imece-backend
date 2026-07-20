using Infrastructure.Database.Connections;
using Infrastructure.Database.Options;
using Infrastructure.Database.Seeding;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Database.Schema;

public interface ISchemaSynchronizer
{
    Task<SchemaSyncResult> SynchronizeAsync(CancellationToken cancellationToken = default);
}
