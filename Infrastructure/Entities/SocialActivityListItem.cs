using Infrastructure.Data;



namespace Infrastructure.Entities;



public sealed class SocialActivityListItem

{

    [DbManager.DbColumn("social_activity_id")]

    public long SocialActivityId { get; set; }



    [DbManager.DbColumn("title")]

    public string Title { get; set; } = string.Empty;



    [DbManager.DbColumn("description")]

    public string? Description { get; set; }



    [DbManager.DbColumn("activity_type")]

    public string ActivityType { get; set; } = string.Empty;



    [DbManager.DbColumn("location")]

    public string? Location { get; set; }



    [DbManager.DbColumn("start_at")]

    public DateTime StartAt { get; set; }



    [DbManager.DbColumn("end_at")]

    public DateTime EndAt { get; set; }



    [DbManager.DbColumn("image_url")]

    public string? ImageUrl { get; set; }



    [DbManager.DbColumn("company_scope")]

    public string CompanyScope { get; set; } = "All";



    [DbManager.DbColumn("company_id")]

    public int? CompanyId { get; set; }



    [DbManager.DbColumn("branch_scope")]

    public string BranchScope { get; set; } = "All";



    [DbManager.DbColumn("branch_id")]

    public int? BranchId { get; set; }



    [DbManager.DbColumn("department_scope")]

    public string DepartmentScope { get; set; } = "All";



    [DbManager.DbColumn("department_id")]

    public int? DepartmentId { get; set; }



    [DbManager.DbColumn("status")]

    public string Status { get; set; } = "Draft";



    [DbManager.DbColumn("is_active")]

    public bool IsActive { get; set; }



    [DbManager.DbColumn("created_by")]

    public int CreatedBy { get; set; }



    [DbManager.DbColumn("created_at")]

    public DateTime CreatedAt { get; set; }



    [DbManager.DbColumn("updated_at")]

    public DateTime UpdatedAt { get; set; }



    [DbManager.DbColumn("company_name")]

    public string? CompanyName { get; set; }



    [DbManager.DbColumn("branch_name")]

    public string? BranchName { get; set; }



    [DbManager.DbColumn("department_name")]

    public string? DepartmentName { get; set; }

}


