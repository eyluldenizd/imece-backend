using System;
using System.Collections.Generic;

namespace Infrastructure.Entities;

public partial class UserRoles
{
    public int UserId { get; set; }

    public int RoleId { get; set; }

    public DateTime AssignedAt { get; set; }

    public virtual Roles Role { get; set; } = null!;

    public virtual Users User { get; set; } = null!;
}
