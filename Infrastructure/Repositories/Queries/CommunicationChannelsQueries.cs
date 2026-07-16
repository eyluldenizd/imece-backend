namespace Infrastructure.Repositories.Queries;

public static class CommunicationChannelsQueries
{
    public const string GetAll = "SELECT channel_id, channel_name, type, address_url, department_in_charge FROM communication_channels;";
}
