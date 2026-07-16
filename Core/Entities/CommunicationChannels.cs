namespace Core.Entities;

public sealed class CommunicationChannels
{
    public long ChannelId { get; set; }
    public string ChannelName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string AddressUrl { get; set; } = string.Empty;
    public string? DepartmentInCharge { get; set; }
}
