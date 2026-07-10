using System;
using System.Collections.Generic;

namespace Infrastructure.Entities;

public partial class Roles
{
    public int RoleId { get; set; }

    public string RoleName { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<UserRoles> UserRoles { get; set; } = new List<UserRoles>();
}
