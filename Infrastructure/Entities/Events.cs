using Infrastructure.Data;



namespace Infrastructure.Entities;



public sealed class Events

{

    [DbManager.DbColumn("event_id")]

    public long EventId { get; set; }



    [DbManager.DbColumn("company_id")]

    public int? CompanyId { get; set; }



    [DbManager.DbColumn("scope_type")]

    public string ScopeType { get; set; } = "Company";



    [DbManager.DbColumn("branch_scope")]

    public string BranchScope { get; set; } = "All";



    [DbManager.DbColumn("branch_id")]

    public int? BranchId { get; set; }



    [DbManager.DbColumn("department_scope")]

    public string DepartmentScope { get; set; } = "All";



    [DbManager.DbColumn("department_id")]

    public int? DepartmentId { get; set; }



    [DbManager.DbColumn("title")]

    public string Title { get; set; } = string.Empty;



    [DbManager.DbColumn("description")]

    public string? Description { get; set; }



    [DbManager.DbColumn("event_type")]

    public string? EventType { get; set; }



    [DbManager.DbColumn("location")]

    public string? Location { get; set; }



    [DbManager.DbColumn("cover_image_url")]

    public string? CoverImageUrl { get; set; }



    [DbManager.DbColumn("start_datetime")]

    public DateTime StartDateTime { get; set; }



    [DbManager.DbColumn("end_datetime")]

    public DateTime EndDateTime { get; set; }



    [DbManager.DbColumn("is_all_day")]

    public bool IsAllDay { get; set; }



    [DbManager.DbColumn("created_by")]

    public int CreatedBy { get; set; }



    [DbManager.DbColumn("created_at")]

    public DateTime CreatedAt { get; set; }

}


