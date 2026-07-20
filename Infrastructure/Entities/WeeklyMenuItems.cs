namespace Infrastructure.Entities;

public sealed class WeeklyMenuItems
{
    public long MenuItemId { get; set; }

    public long MenuId { get; set; }

    public DateOnly MenuDate { get; set; }

    public int DishCategoryId { get; set; }

    public int DishId { get; set; }

    public int SortOrder { get; set; }

    public string? Notes { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
