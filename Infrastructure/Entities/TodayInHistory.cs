using System;

namespace Infrastructure.Entities;

public partial class TodayInHistory
{
    public long Id { get; set; }

    public DateTime EventDate { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    public DateTime CreatedAt { get; set; }
}