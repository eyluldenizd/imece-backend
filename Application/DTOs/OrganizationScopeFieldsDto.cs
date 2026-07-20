namespace Application.DTOs;

public class OrganizationScopeFieldsDto
{
    public string CompanyScope { get; set; } = "All";

    public int? CompanyId { get; set; }

    public string BranchScope { get; set; } = "All";

    public int? BranchId { get; set; }

    public string DepartmentScope { get; set; } = "All";

    public int? DepartmentId { get; set; }
}
