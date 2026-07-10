using System;
using System.Collections.Generic;

namespace Infrastructure.Entities;

public partial class Dishes
{
    public int DishId { get; set; }

    public string DishName { get; set; } = null!;

    public string Category { get; set; } = null!;

    public int? Calorie { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<WeeklyMenuEntries> WeeklyMenuEntries { get; set; } = new List<WeeklyMenuEntries>();
}
