namespace Infrastructure.Database.Connections;

public interface IConnectionStringFactory
{
    string GetApplicationConnectionString();

    string GetMasterConnectionString();
}
