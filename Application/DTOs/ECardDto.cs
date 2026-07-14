namespace Application.DTOs;

public class ECardDto
{
    public long ECardId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string? CardType { get; set; }

    public string? ImageUrl { get; set; }

    public string? RedirectUrl { get; set; }

    public bool IsActive { get; set; }

    public int? DisplayOrder { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}