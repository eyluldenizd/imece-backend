using System;
using System.Collections.Generic;

namespace Infrastructure.Entities;

public partial class WeeklyMenuEntries
{
    public long EntryId { get; set; }

    public int WeekId { get; set; }

    public int DishId { get; set; }

    public int BranchId { get; set; }

    public DateOnly MenuDate { get; set; }

    public string MealType { get; set; } = null!;

    public short SortOrder { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Branches Branch { get; set; } = null!;

    public virtual Users? CreatedByNavigation { get; set; }

    public virtual Dishes Dish { get; set; } = null!;

    public virtual Weeks Week { get; set; } = null!;
}
