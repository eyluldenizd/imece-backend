using Infrastructure.Data;
using System;
using System.Collections.Generic;

namespace Infrastructure.Entities;

public partial class Photos
{
    [DbManager.DbColumn("photo_id")]
    public long PhotoId { get; set; }

    [DbManager.DbColumn("album_id")]
    public int AlbumId { get; set; }

    [DbManager.DbColumn("file_url")]
    public string FileUrl { get; set; } = null!;

    [DbManager.DbColumn("thumbnail_url")]
    public string? ThumbnailUrl { get; set; }

    [DbManager.DbColumn("caption")]
    public string? Caption { get; set; }

    [DbManager.DbColumn("uploaded_by")]
    public int UploadedBy { get; set; }

    [DbManager.DbColumn("uploaded_at")]
    public DateTime UploadedAt { get; set; }

    [DbManager.DbColumn("sort_order")]
    public int SortOrder { get; set; }

    public virtual PhotoAlbums Album { get; set; } = null!;

    public virtual ICollection<PhotoAlbums> PhotoAlbums { get; set; } = new List<PhotoAlbums>();

    public virtual Users UploadedByNavigation { get; set; } = null!;
}
