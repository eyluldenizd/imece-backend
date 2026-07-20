using Infrastructure.Data;
using System;
using System.Collections.Generic;

namespace Infrastructure.Entities;

public partial class Dishes
{
    [DbManager.DbColumn("dish_id")]
    public int DishId { get; set; }

    [DbManager.DbColumn("dish_name")]       
    public string DishName { get; set; } = null!;

    [DbManager.DbColumn("dish_category_id")]
    public int? DishCategoryId { get; set; }
    
    [DbManager.DbColumn("category")]
    public string Category { get; set; } = null!;

    [DbManager.DbColumn("description")]
    public string? Description { get; set; }

    [DbManager.DbColumn("image_url")]
    public string? ImageUrl { get; set; }
    
    [DbManager.DbColumn("is_active")]
    public bool IsActive { get; set; }
    
    [DbManager.DbColumn("created_at")]
    public DateTime CreatedAt { get; set; }

    [DbManager.DbColumn("updated_at")]
    public DateTime UpdatedAt { get; set; }
    
    public virtual ICollection<WeeklyMenuEntries> WeeklyMenuEntries { get; set; } = new List<WeeklyMenuEntries>();
}
