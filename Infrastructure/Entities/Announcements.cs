using System;
using System.Collections.Generic;

namespace Infrastructure.Entities;

public partial class Announcements
{
    public long AnnouncementId { get; set; }

    public string Title { get; set; } = null!;

    public string Content { get; set; } = null!;

    public string? CoverImageUrl { get; set; }

    public int AuthorUserId { get; set; }

    public bool IsPinned { get; set; }

    public DateTime PublishStart { get; set; }

    public DateTime? PublishEnd { get; set; }

    public int ViewCount { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Users AuthorUser { get; set; } = null!;
}
