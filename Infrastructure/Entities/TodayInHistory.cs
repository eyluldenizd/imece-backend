using Infrastructure.Data;

namespace Infrastructure.Entities;

public partial class TodayInHistory
{
    public long Id { get; set; }

    [DbManager.DbColumn("event_date")]
    public DateTime EventDate { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    [DbManager.DbColumn("image_url")]
    public string? ImageUrl { get; set; }

    [DbManager.DbColumn("created_at")]
    public DateTime CreatedAt { get; set; }
}
