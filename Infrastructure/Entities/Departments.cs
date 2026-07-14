using Infrastructure.Data;
using System;
using System.Collections.Generic;

namespace Infrastructure.Entities;

public partial class Departments
{
    [DbManager.DbColumn("department_id")]
    public int DepartmentId { get; set; }

    [DbManager.DbColumn("parent_department_id")]
    public int? ParentDepartmentId { get; set; }

    [DbManager.DbColumn("department_code")]
    public string? DepartmentCode { get; set; }

    [DbManager.DbColumn("department_name")]
    public string DepartmentName { get; set; } = null!;
    
    [DbManager.DbColumn("is_active")]
    public bool IsActive { get; set; }

    public virtual ICollection<Departments> InverseParentDepartment { get; set; } = new List<Departments>();

    public virtual Departments? ParentDepartment { get; set; }

    public virtual ICollection<Users> Users { get; set; } = new List<Users>();
}
