namespace Core.Entities;

public sealed class CommunicationChannels
{
    public long ChannelId { get; set; }
    public string ChannelName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int? CommunicationChannelTypeId { get; set; }
    public string AddressUrl { get; set; } = string.Empty;
    public string? DepartmentInCharge { get; set; }
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public string CompanyScope { get; set; } = "All";
    public int? CompanyId { get; set; }
    public string BranchScope { get; set; } = "All";
    public int? BranchId { get; set; }
    public string DepartmentScope { get; set; } = "All";
    public int? DepartmentId { get; set; }
    public string? CompanyName { get; set; }
    public string? BranchName { get; set; }
    public string? DepartmentName { get; set; }
    public string? TypeIconUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
