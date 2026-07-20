using Infrastructure.Data;

namespace Infrastructure.Entities;

public sealed class ServiceLocations
{
    [DbManager.DbColumn("service_location_id")]
    public long ServiceLocationId { get; set; }

    [DbManager.DbColumn("company_id")]
    public int? CompanyId { get; set; }

    [DbManager.DbColumn("branch_id")]
    public int? BranchId { get; set; }

    [DbManager.DbColumn("name")]
    public string Name { get; set; } = string.Empty;

    [DbManager.DbColumn("service_location_type_id")]
    public int? ServiceLocationTypeId { get; set; }

    [DbManager.DbColumn("location_type")]
    public string LocationType { get; set; } = string.Empty;

    [DbManager.DbColumn("type_name")]
    public string? TypeName { get; set; }

    [DbManager.DbColumn("address")]
    public string? Address { get; set; }

    [DbManager.DbColumn("latitude")]
    public decimal? Latitude { get; set; }

    [DbManager.DbColumn("longitude")]
    public decimal? Longitude { get; set; }

    [DbManager.DbColumn("is_active")]
    public bool IsActive { get; set; }

    [DbManager.DbColumn("created_at")]
    public DateTime CreatedAt { get; set; }

    [DbManager.DbColumn("updated_at")]
    public DateTime UpdatedAt { get; set; }
}
