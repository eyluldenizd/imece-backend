using Infrastructure.Data;

namespace Infrastructure.Entities;

public partial class ECards
{
    [DbManager.DbColumn("e_card_id")]
    public long ECardId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    [DbManager.DbColumn("card_type")]
    public string? CardType { get; set; }

    [DbManager.DbColumn("image_url")]
    public string? ImageUrl { get; set; }

    [DbManager.DbColumn("redirect_url")]
    public string? RedirectUrl { get; set; }

    [DbManager.DbColumn("is_active")]
    public bool IsActive { get; set; }

    [DbManager.DbColumn("display_order")]
    public int? DisplayOrder { get; set; }

    [DbManager.DbColumn("created_at")]
    public DateTime CreatedAt { get; set; }

    [DbManager.DbColumn("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}
