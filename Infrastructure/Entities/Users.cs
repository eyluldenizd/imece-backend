using Infrastructure.Data;

namespace Infrastructure.Entities;

public partial class Users
{
    [DbManager.DbColumn("user_id")]
    public int UserId { get; set; }

    [DbManager.DbColumn("azure_object_id")]
    public string AzureObjectId { get; set; } = null!;

    [DbManager.DbColumn("email")]
    public string Email { get; set; } = null!;

    [DbManager.DbColumn("full_name")]
    public string FullName { get; set; } = null!;

    [DbManager.DbColumn("title")]
    public string? Title { get; set; }

    [DbManager.DbColumn("department_id")]
    public int? DepartmentId { get; set; }

    [DbManager.DbColumn("branch_id")]
    public int? BranchId { get; set; }

    [DbManager.DbColumn("role_id")]
    public int RoleId { get; set; }

    [DbManager.DbColumn("birth_date")]
    public DateOnly? BirthDate { get; set; }

    [DbManager.DbColumn("birth_month")]
    public int? BirthMonth { get; set; }

    [DbManager.DbColumn("birth_day")]
    public int? BirthDay { get; set; }

    [DbManager.DbColumn("hire_date")]
    public DateOnly? HireDate { get; set; }

    [DbManager.DbColumn("phone")]
    public string? Phone { get; set; }

    [DbManager.DbColumn("photo_url")]
    public string? PhotoUrl { get; set; }

    [DbManager.DbColumn("is_active")]
    public bool IsActive { get; set; }

    [DbManager.DbColumn("last_login_at")]
    public DateTime? LastLoginAt { get; set; }

    [DbManager.DbColumn("created_at")]
    public DateTime CreatedAt { get; set; }

    [DbManager.DbColumn("updated_at")]
    public DateTime UpdatedAt { get; set; }

    public virtual Roles Role { get; set; } = null!;

    public virtual Branches? Branch { get; set; }

    public virtual Departments? Department { get; set; }

    public virtual ICollection<Announcements> Announcements { get; set; }
        = new List<Announcements>();

    public virtual ICollection<EventParticipants> EventParticipants { get; set; }
        = new List<EventParticipants>();

    public virtual ICollection<Events> Events { get; set; }
        = new List<Events>();

    public virtual ICollection<WeeklyMenuEntries> WeeklyMenuEntries { get; set; }
        = new List<WeeklyMenuEntries>();
}