using Infrastructure.Data;
using System;
using System.Collections.Generic;

namespace Infrastructure.Entities;

public partial class Branches
{
    
    [DbManager.DbColumn("branch_id")]
    public int BranchId { get; set; }

    [DbManager.DbColumn("branch_code")]
    public string BranchCode { get; set; } = null!;

    [DbManager.DbColumn("branch_name")]
    public string BranchName { get; set; } = null!;

    [DbManager.DbColumn("address")]
    public string? Address { get; set; }

    [DbManager.DbColumn("latitude")]
    public decimal? Latitude { get; set; }

    [DbManager.DbColumn("longitude")]
    public decimal? Longitude { get; set; }

    [DbManager.DbColumn("is_active")]
    public bool IsActive { get; set; }
    
    [DbManager.DbColumn("created_at")]
    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Users> Users { get; set; } = new List<Users>();

    public virtual ICollection<WeeklyMenuEntries> WeeklyMenuEntries { get; set; } = new List<WeeklyMenuEntries>();
}
