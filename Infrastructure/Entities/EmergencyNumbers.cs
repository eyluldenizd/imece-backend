using Infrastructure.Data;

namespace Infrastructure.Entities;

public partial class EmergencyNumbers
{
    [DbManager.DbColumn("emergency_number_id")]
    public long EmergencyNumberId { get; set; }

    public string Name { get; set; } = null!;

    [DbManager.DbColumn("phone_number")]
    public string PhoneNumber { get; set; } = null!;

    public string Category { get; set; } = null!;

    public string? Description { get; set; }

    [DbManager.DbColumn("is_active")]
    public bool IsActive { get; set; }

    [DbManager.DbColumn("display_order")]
    public int? DisplayOrder { get; set; }

    [DbManager.DbColumn("created_at")]
    public DateTime CreatedAt { get; set; }

    [DbManager.DbColumn("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}
