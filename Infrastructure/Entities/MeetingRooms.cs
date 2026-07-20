using Infrastructure.Data;

namespace Infrastructure.Entities;

public sealed class MeetingRooms
{
    [DbManager.DbColumn("meeting_room_id")]
    public int MeetingRoomId { get; set; }

    [DbManager.DbColumn("company_id")]
    public int CompanyId { get; set; }

    [DbManager.DbColumn("branch_id")]
    public int? BranchId { get; set; }

    [DbManager.DbColumn("department_id")]
    public int? DepartmentId { get; set; }

    [DbManager.DbColumn("name")]
    public string Name { get; set; } = string.Empty;

    [DbManager.DbColumn("code")]
    public string Code { get; set; } = string.Empty;

    [DbManager.DbColumn("floor")]
    public string? Floor { get; set; }

    [DbManager.DbColumn("capacity")]
    public int Capacity { get; set; }

    [DbManager.DbColumn("location_description")]
    public string? LocationDescription { get; set; }

    [DbManager.DbColumn("features")]
    public string? Features { get; set; }

    [DbManager.DbColumn("is_active")]
    public bool IsActive { get; set; }

    [DbManager.DbColumn("created_at")]
    public DateTime CreatedAt { get; set; }

    [DbManager.DbColumn("updated_at")]
    public DateTime UpdatedAt { get; set; }
}
