using Infrastructure.Data;
using System;
using System.Collections.Generic;

namespace Infrastructure.Entities;

public partial class Roles
{
    [DbManager.DbColumn("RoleId")]
    public int RoleId { get; set; }
    
    [DbManager.DbColumn("RoleName")]
    public string RoleName { get; set; } = null!;

    [DbManager.DbColumn("Description")]
    public string? Description { get; set; }

    public virtual ICollection<UserRoles> UserRoles { get; set; } = new List<UserRoles>();
}
