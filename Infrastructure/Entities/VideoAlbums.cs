using Infrastructure.Data;
using System;
using System.Collections.Generic;

namespace Infrastructure.Entities;

public partial class VideoAlbums
{
    [DbManager.DbColumn("video_album_id")]
    public int VideoAlbumId { get; set; }

    [DbManager.DbColumn("title")]
    public string Title { get; set; } = null!;

    [DbManager.DbColumn("description")]
    public string? Description { get; set; }
    
    [DbManager.DbColumn("created_by")]
    public int CreatedBy { get; set; }
    
    [DbManager.DbColumn("created_at")]
    public DateTime CreatedAt { get; set; }

    public virtual Users CreatedByNavigation { get; set; } = null!;

    public virtual ICollection<Videos> Videos { get; set; } = new List<Videos>();
}
