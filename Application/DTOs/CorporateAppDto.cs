namespace Application.DTOs;

public sealed class CorporateAppDto
{
    public long AppId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? Category { get; set; }
}
