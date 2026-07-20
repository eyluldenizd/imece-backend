namespace Application.DTOs;

public sealed class CommunicationChannelDto
{
    public long ChannelId { get; set; }
    public string ChannelName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string AddressUrl { get; set; } = string.Empty;
    public string? DepartmentInCharge { get; set; }
}

public sealed class CreateCommunicationChannelDto
{
    public string ChannelName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string AddressUrl { get; set; } = string.Empty;
    public string? DepartmentInCharge { get; set; }
}

public sealed class UpdateCommunicationChannelDto
{
    public long ChannelId { get; set; }
    public string ChannelName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string AddressUrl { get; set; } = string.Empty;
    public string? DepartmentInCharge { get; set; }
}
