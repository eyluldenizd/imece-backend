namespace Application.DTOs;

/// <summary>
/// Admin liste sayfaları için ortak sorgu parametreleri.
/// </summary>
public sealed class ContentListQueryDto
{
    public string? Search { get; set; }
    public int? CompanyId { get; set; }
    public string? CompanyName { get; set; }
    public int? BranchId { get; set; }
    public int? DepartmentId { get; set; }
    public string? RoleName { get; set; }
    public bool? IsPinned { get; set; }
    public bool? IsActive { get; set; }
    public string? ScopeType { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? SortBy { get; set; }
    public string? SortDir { get; set; }

    /// <summary>Rezervasyon durumu, etkinlik yaşam döngüsü (upcoming/ongoing/completed), sosyal aktivite durumu.</summary>
    public string? Status { get; set; }

    /// <summary>Kategori FK (yemek, kurumsal uygulama, kanal türü, konum türü vb.).</summary>
    public int? CategoryId { get; set; }

    /// <summary>Genel tür alanı (eventType, activityType, mediaType, kanal/konum türü, pdf/doc).</summary>
    public string? Type { get; set; }

    public string? Floor { get; set; }
    public string? Feature { get; set; }
    public string? RoomName { get; set; }
    public string? ColorKey { get; set; }
    public string? Departure { get; set; }
    public string? Arrival { get; set; }
    public bool? IsPublished { get; set; }

    /// <summary>Medya özellik türü (Document, Gallery, Media).</summary>
    public string? FeatureType { get; set; }

    public int? MeetingRoomId { get; set; }
    public int? Year { get; set; }
    public int? Month { get; set; }

    /// <summary>1-based page index for server-side pagination.</summary>
    public int? Page { get; set; }

    /// <summary>Page size for server-side pagination (clamped server-side).</summary>
    public int? PageSize { get; set; }
}
