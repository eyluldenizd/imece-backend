using System;
using System.Collections.Generic;

namespace Infrastructure.Entities;

public partial class Departments
{
    public int DepartmentId { get; set; }

    public int? ParentDepartmentId { get; set; }

    public string? DepartmentCode { get; set; }

    public string DepartmentName { get; set; } = null!;

    public bool IsActive { get; set; }

    public virtual ICollection<Departments> InverseParentDepartment { get; set; } = new List<Departments>();

    public virtual Departments? ParentDepartment { get; set; }

    public virtual ICollection<Users> Users { get; set; } = new List<Users>();
}
