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
    public bool? IsPinned { get; set; }
    public bool? IsActive { get; set; }
    public string? ScopeType { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? SortBy { get; set; }
    public string? SortDir { get; set; }
}
