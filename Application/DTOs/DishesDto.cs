namespace Application.DTOs;

public sealed class DishesDto
{
    public int DishId { get; set; }

    public string DishName { get; set; } = null!;

    public string Category { get; set; } = null!;

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }
}

public sealed class CreateDishesDto
{
    public string DishName { get; set; } = null!;

    public string Category { get; set; } = null!;

    public bool IsActive { get; set; } = true;
}

public sealed class UpdateDishesDto
{
    public int DishId { get; set; }

    public string DishName { get; set; } = null!;

    public string Category { get; set; } = null!;

    public bool IsActive { get; set; }
}