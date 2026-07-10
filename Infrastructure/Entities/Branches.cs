using System;
using System.Collections.Generic;

namespace Infrastructure.Entities;

public partial class Branches
{
    public int BranchId { get; set; }

    public string BranchCode { get; set; } = null!;

    public string BranchName { get; set; } = null!;

    public string? Address { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Users> Users { get; set; } = new List<Users>();

    public virtual ICollection<WeeklyMenuEntries> WeeklyMenuEntries { get; set; } = new List<WeeklyMenuEntries>();
}
