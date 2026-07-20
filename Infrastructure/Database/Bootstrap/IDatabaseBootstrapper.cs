using Infrastructure.Database.Connections;
using Infrastructure.Database.Options;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Database.Bootstrap;

public interface IDatabaseBootstrapper
{
    /// <summary>
    /// Veritabanı yoksa (ve izin varsa) oluşturur. true = yeni oluşturuldu.
    /// </summary>
    Task<bool> EnsureDatabaseAsync(CancellationToken cancellationToken = default);
}
