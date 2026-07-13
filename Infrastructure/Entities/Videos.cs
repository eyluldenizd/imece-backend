using Infrastructure.Data;
using System;
using System.Collections.Generic;

namespace Infrastructure.Entities;

public partial class Videos
{
    [DbManager.DbColumn("video_id")]
    public long VideoId { get; set; }

    [DbManager.DbColumn("video_album_id")]
    public int VideoAlbumId { get; set; }

    [DbManager.DbColumn("title")]
    public string Title { get; set; } = null!;

    [DbManager.DbColumn("description")]
    public string VideoSource { get; set; } = null!;

    [DbManager.DbColumn("video_url")]
    public string VideoUrl { get; set; } = null!;

    [DbManager.DbColumn("thumbnail_url")]
    public string? ThumbnailUrl { get; set; }

    [DbManager.DbColumn("duration_seconds")]
    public int? DurationSeconds { get; set; }

    [DbManager.DbColumn("uploaded_by")]
    public int UploadedBy { get; set; }

    [DbManager.DbColumn("uploaded_at")]
    public DateTime UploadedAt { get; set; }

    [DbManager.DbColumn("sort_order")]
    public int SortOrder { get; set; }

    public virtual Users UploadedByNavigation { get; set; } = null!;

    public virtual VideoAlbums VideoAlbum { get; set; } = null!;
}
