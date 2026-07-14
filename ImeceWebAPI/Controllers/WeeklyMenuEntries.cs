using Infrastructure.Data;
using System;
using System.Collections.Generic;

namespace Infrastructure.Entities;

public partial class WeeklyMenuEntries
{
    [DbManager.DbColumn("entry_id")]
    public long EntryId { get; set; }

    [DbManager.DbColumn("week_id")]
    public int WeekId { get; set; }

    [DbManager.DbColumn("dish_id")]
    public int DishId { get; set; }

    [DbManager.DbColumn("branch_id")]
    public int BranchId { get; set; }

    [DbManager.DbColumn("menu_date")]
    public DateOnly MenuDate { get; set; }

    [DbManager.DbColumn("meal_type")]
    public string MealType { get; set; } = null!;

    [DbManager.DbColumn("sort_order")]
    public short SortOrder { get; set; }

    [DbManager.DbColumn("created_by")]
    public int? CreatedBy { get; set; }

    [DbManager.DbColumn("created_at")]
    public DateTime CreatedAt { get; set; }

    public virtual Branches Branch { get; set; } = null!;

    public virtual Users? CreatedByNavigation { get; set; }

    public virtual Dishes Dish { get; set; } = null!;

    public virtual Weeks Week { get; set; } = null!;
}