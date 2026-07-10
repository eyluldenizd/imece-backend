using System;
using System.Collections.Generic;

namespace Infrastructure.Entities;

public partial class Weeks
{
    public int WeekId { get; set; }

    public short Year { get; set; }

    public byte WeekNumber { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public virtual ICollection<WeeklyMenuEntries> WeeklyMenuEntries { get; set; } = new List<WeeklyMenuEntries>();
}
