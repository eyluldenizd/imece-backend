using Infrastructure.Database.Options;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Infrastructure.Database.Connections;

public sealed class ConnectionStringFactory : IConnectionStringFactory
{
    private readonly DatabaseOptions _options;
    private readonly string? _fallbackConnectionString;

    public ConnectionStringFactory(
        IOptions<DatabaseOptions> options,
        IConfiguration configuration)
    {
        _options = options.Value;
        _fallbackConnectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public string GetApplicationConnectionString()
    {
        if (CanBuildFromOptions())
        {
            return BuildConnectionString(_options.DatabaseName);
        }

        if (!string.IsNullOrWhiteSpace(_fallbackConnectionString))
        {
            return _fallbackConnectionString;
        }

        throw new InvalidOperationException(
            "Database bağlantı dizesi oluşturulamadı. Database seçeneklerini veya ConnectionStrings:DefaultConnection değerini yapılandırın.");
    }

    public string GetMasterConnectionString()
    {
        if (CanBuildFromOptions())
        {
            return BuildConnectionString("master");
        }

        if (!string.IsNullOrWhiteSpace(_fallbackConnectionString))
        {
            var builder = new SqlConnectionStringBuilder(_fallbackConnectionString)
            {
                InitialCatalog = "master"
            };
            return builder.ConnectionString;
        }

        throw new InvalidOperationException(
            "Master bağlantı dizesi oluşturulamadı. Database seçeneklerini veya ConnectionStrings:DefaultConnection değerini yapılandırın.");
    }

    private bool CanBuildFromOptions() =>
        !string.IsNullOrWhiteSpace(_options.Server)
        && !string.IsNullOrWhiteSpace(_options.DatabaseName);

    private string BuildConnectionString(string databaseName)
    {
        var dataSource = string.IsNullOrWhiteSpace(_options.Instance)
            ? _options.Server
            : $"{_options.Server}\\{_options.Instance}";

        var builder = new SqlConnectionStringBuilder
        {
            DataSource = dataSource,
            InitialCatalog = databaseName,
            Encrypt = _options.Encrypt,
            TrustServerCertificate = _options.TrustServerCertificate,
            ConnectTimeout = _options.ConnectTimeoutSeconds,
            IntegratedSecurity = _options.IntegratedSecurity
        };

        if (!_options.IntegratedSecurity)
        {
            builder.UserID = _options.UserName ?? string.Empty;
            builder.Password = _options.Password ?? string.Empty;
        }

        return builder.ConnectionString;
    }
}
