using Infrastructure.Data;
using System;
using System.Collections.Generic;

namespace Infrastructure.Entities;

public partial class Announcements
{
    [DbManager.DbColumn("announcement_id")]
    public long AnnouncementId { get; set; }

    [DbManager.DbColumn("title")]
    public string Title { get; set; } = null!;

    [DbManager.DbColumn("content")] 
    public string Content { get; set; } = null!;
    
    [DbManager.DbColumn("cover_image_url")]
    public string? CoverImageUrl { get; set; }
    
   // [DbManager.DbColumn("author_user_id")]
   //public int AuthorUserId { get; set; }

    [DbManager.DbColumn("is_pinned")]
    public bool IsPinned { get; set; }

    [DbManager.DbColumn("publish_start")]
    public DateTime PublishStart { get; set; }

    [DbManager.DbColumn("publish_end")]
    public DateTime? PublishEnd { get; set; }

    [DbManager.DbColumn("view_count")]
    public int ViewCount { get; set; }

    [DbManager.DbColumn("created_at")]
    public DateTime CreatedAt { get; set; }

    [DbManager.DbColumn("updated_at")]
    public DateTime UpdatedAt { get; set; }

    
    public virtual Users AuthorUser { get; set; } = null!;
}
