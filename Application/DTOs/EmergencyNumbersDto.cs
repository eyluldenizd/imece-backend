namespace Application.DTOs;

public class EmergencyNumberDto : OrganizationScopeFieldsDto
{
    public long EmergencyNumberId { get; set; }
    public string Name { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string Category { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public int? DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
