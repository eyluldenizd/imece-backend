using Infrastructure.Data;
using System;
using System.Collections.Generic;

namespace Infrastructure.Entities;

public partial class UserRoles
{
    [DbManager.DbColumn("user_role_id")]
    public int UserId { get; set; }

    [DbManager.DbColumn("role_id")]
    public int RoleId { get; set; }

    [DbManager.DbColumn("assigned_at")]
    public DateTime AssignedAt { get; set; }

    public virtual Roles Role { get; set; } = null!;

    public virtual Users User { get; set; } = null!;
}
