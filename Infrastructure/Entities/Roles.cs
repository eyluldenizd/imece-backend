using Infrastructure.Data;

namespace Infrastructure.Entities;

public partial class Roles
{
    [DbManager.DbColumn("role_id")]
    public int RoleId { get; set; }

    [DbManager.DbColumn("role_name")]
    public string RoleName { get; set; } = null!;

    [DbManager.DbColumn("description")]
    public string? Description { get; set; }

    [DbManager.DbColumn("is_active")]
    public bool IsActive { get; set; }

    public virtual ICollection<Users> Users { get; set; }
        = new List<Users>();
}