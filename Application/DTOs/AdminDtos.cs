namespace Application.DTOs;

public sealed class CompanyIdRequest { public int CompanyId { get; set; } }
public sealed class BranchIdRequest { public int BranchId { get; set; } }
public sealed class WeeklyMenuRouteRequest { public long MenuId { get; set; } }
public sealed class WeeklyMenuItemRouteRequest { public long MenuId { get; set; } public long MenuItemId { get; set; } }
public sealed class UpdateRolePermissionsRequest { public int RoleId { get; set; } public IReadOnlyCollection<int> PermissionIds { get; set; } = []; }

public record CurrentUserCompanyResponse(int CompanyId, string CompanyName, IReadOnlyCollection<string> Roles);
public record CurrentUserResponse(int UserId, string Username, string Email, string DisplayName, int? ActiveCompanyId,
    string? ActiveCompanyName, IReadOnlyCollection<string> Roles, IReadOnlyCollection<string> Permissions,
    IReadOnlyCollection<CurrentUserCompanyResponse> Companies, bool HasAdminPanelAccess);
public sealed class LoginRequestDto { public string Username { get; set; } = string.Empty; public string Password { get; set; } = string.Empty; }
public sealed class LoginResponseDto { public string AccessToken { get; set; } = string.Empty; public DateTime ExpiresAt { get; set; } public CurrentUserResponse User { get; set; } = null!; }

public sealed class CreateBranchDto { public int CompanyId { get; set; } public string BranchCode { get; set; } = string.Empty; public string BranchName { get; set; } = string.Empty; public bool IsActive { get; set; } = true; }
public sealed class UpdateBranchDto { public int BranchId { get; set; } public int CompanyId { get; set; } public string BranchCode { get; set; } = string.Empty; public string BranchName { get; set; } = string.Empty; public bool IsActive { get; set; } }
public sealed class CreateCompanyDto { public string CompanyCode { get; set; } = string.Empty; public string CompanyName { get; set; } = string.Empty; public bool IsActive { get; set; } = true; }
public sealed class UpdateCompanyDto { public int CompanyId { get; set; } public string CompanyCode { get; set; } = string.Empty; public string CompanyName { get; set; } = string.Empty; public bool IsActive { get; set; } }
public sealed class CreateDepartmentDto { public int CompanyId { get; set; } public int? BranchId { get; set; } public string DepartmentName { get; set; } = string.Empty; public bool IsActive { get; set; } = true; }
public sealed class UpdateDepartmentDto { public int DepartmentId { get; set; } public int CompanyId { get; set; } public int? BranchId { get; set; } public string DepartmentName { get; set; } = string.Empty; public bool IsActive { get; set; } }
public sealed class CreateRoleDto { public string RoleName { get; set; } = string.Empty; public string? Description { get; set; } }
public sealed class UpdateRoleDto { public int RoleId { get; set; } public string RoleName { get; set; } = string.Empty; public string? Description { get; set; } }
public sealed class UpdateRolePermissionsDto { public IReadOnlyCollection<int> PermissionIds { get; set; } = []; }

public sealed class CreateCommunicationChannelTypeDto { public string Name { get; set; } = string.Empty; }
public sealed class UpdateCommunicationChannelTypeDto { public int CommunicationChannelTypeId { get; set; } public string Name { get; set; } = string.Empty; }
public sealed class CreateCorporateAppCategoryDto { public string Name { get; set; } = string.Empty; }
public sealed class UpdateCorporateAppCategoryDto { public int CorporateAppCategoryId { get; set; } public string Name { get; set; } = string.Empty; }
public sealed class CreateDishCategoryDto { public string Name { get; set; } = string.Empty; }
public sealed class UpdateDishCategoryDto { public int DishCategoryId { get; set; } public string Name { get; set; } = string.Empty; }
public sealed class CreateServiceLocationTypeDto { public string Name { get; set; } = string.Empty; }
public sealed class UpdateServiceLocationTypeDto { public int ServiceLocationTypeId { get; set; } public string Name { get; set; } = string.Empty; }

public sealed class CreateMeetingRoomDto { public int CompanyId { get; set; } public string Name { get; set; } = string.Empty; public int Capacity { get; set; } public bool IsActive { get; set; } = true; }
public sealed class UpdateMeetingRoomDto { public int MeetingRoomId { get; set; } public int CompanyId { get; set; } public string Name { get; set; } = string.Empty; public int Capacity { get; set; } public bool IsActive { get; set; } }
public sealed class CreateSocialActivityDto { public int CompanyId { get; set; } public string Title { get; set; } = string.Empty; public string? Description { get; set; } public DateTime StartDate { get; set; } public DateTime? EndDate { get; set; } public string? Location { get; set; } }
public sealed class UpdateSocialActivityDto { public long SocialActivityId { get; set; } public int CompanyId { get; set; } public string Title { get; set; } = string.Empty; public string? Description { get; set; } public DateTime StartDate { get; set; } public DateTime? EndDate { get; set; } public string? Location { get; set; } }
public sealed class CreateServiceLocationDto { public int CompanyId { get; set; } public int? ServiceLocationTypeId { get; set; } public string Name { get; set; } = string.Empty; public string? Address { get; set; } public bool IsActive { get; set; } = true; }
public sealed class UpdateServiceLocationDto { public long ServiceLocationId { get; set; } public int CompanyId { get; set; } public int? ServiceLocationTypeId { get; set; } public string Name { get; set; } = string.Empty; public string? Address { get; set; } public bool IsActive { get; set; } }

public sealed class CreateWeeklyMenuDto { public int CompanyId { get; set; } public int BranchId { get; set; } public DateTime StartDate { get; set; } public DateTime EndDate { get; set; } }
public sealed class UpdateWeeklyMenuDto { public long MenuId { get; set; } public int CompanyId { get; set; } public int BranchId { get; set; } public DateTime StartDate { get; set; } public DateTime EndDate { get; set; } }
public sealed class CreateWeeklyMenuItemDto { public long MenuId { get; set; } public int DishId { get; set; } public DateTime MenuDate { get; set; } public string MealType { get; set; } = string.Empty; public short SortOrder { get; set; } }
public sealed class UpdateWeeklyMenuItemDto { public long MenuId { get; set; } public long MenuItemId { get; set; } public int DishId { get; set; } public DateTime MenuDate { get; set; } public string MealType { get; set; } = string.Empty; public short SortOrder { get; set; } }
