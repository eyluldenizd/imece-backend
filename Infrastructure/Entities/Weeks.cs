using Infrastructure.Data;
using System;
using System.Collections.Generic;

namespace Infrastructure.Entities;

public partial class Weeks
{
    [DbManager.DbColumn("week_id")]
    public int WeekId { get; set; }
    
    [DbManager.DbColumn("year")]
    public short Year { get; set; }
    
    [DbManager.DbColumn("week_number")]
    public byte WeekNumber { get; set; }
    
    [DbManager.DbColumn("start_date")]
    public DateOnly StartDate { get; set; }
    
    [DbManager.DbColumn("end_date")]
    public DateOnly EndDate { get; set; }

    public virtual ICollection<WeeklyMenuEntries> WeeklyMenuEntries { get; set; } = new List<WeeklyMenuEntries>();
}
