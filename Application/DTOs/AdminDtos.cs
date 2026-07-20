using Core.Common.Validation;

namespace Application.DTOs;

// --- AuthDtos.cs ---
public sealed record LoginRequestDto(string Username, string Password);

public sealed record LoginResponseDto(
    string AccessToken,
    DateTime ExpiresAt,
    string TokenType,
    CurrentUserResponse User);

// --- BranchDto.cs ---
public sealed class BranchDto
{
    public int BranchId { get; set; }

    public int? CompanyId { get; set; }

    public string BranchCode { get; set; } = string.Empty;

    public string BranchName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Address { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

public sealed class CreateBranchDto
{
    [Validate(
        ValidationRuleType.GreaterThan,
        0,
        ErrorMessage = "┼Şirket ID zorunludur.")]
    public int CompanyId { get; set; }

    [Validate(
        ValidationRuleType.MaxLength,
        64,
        ErrorMessage = "┼Şube kodu en fazla 64 karakter olabilir.")]
    public string? BranchCode { get; set; }

    [Validate(
        ValidationRuleType.Required,
        ErrorMessage = "┼Şube ad─▒ zorunludur.")]
    [Validate(
        ValidationRuleType.MaxLength,
        256,
        ErrorMessage = "┼Şube ad─▒ en fazla 256 karakter olabilir.")]
    public string BranchName { get; set; } = string.Empty;

    [Validate(
        ValidationRuleType.MaxLength,
        512,
        ErrorMessage = "A├ğ─▒klama en fazla 512 karakter olabilir.")]
    public string? Description { get; set; }

    [Validate(
        ValidationRuleType.MaxLength,
        512,
        ErrorMessage = "Adres en fazla 512 karakter olabilir.")]
    public string? Address { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public bool IsActive { get; set; } = true;
}

public sealed class UpdateBranchDto
{
    [Validate(
        ValidationRuleType.GreaterThan,
        0,
        ErrorMessage = "Ge├ğerli bir ┼şube ID de─şeri g├Ânderilmelidir.")]
    public int BranchId { get; set; }

    [Validate(
        ValidationRuleType.GreaterThan,
        0,
        ErrorMessage = "┼Şirket ID zorunludur.")]
    public int CompanyId { get; set; }

    [Validate(
        ValidationRuleType.MaxLength,
        64,
        ErrorMessage = "┼Şube kodu en fazla 64 karakter olabilir.")]
    public string? BranchCode { get; set; }

    [Validate(
        ValidationRuleType.Required,
        ErrorMessage = "┼Şube ad─▒ zorunludur.")]
    [Validate(
        ValidationRuleType.MaxLength,
        256,
        ErrorMessage = "┼Şube ad─▒ en fazla 256 karakter olabilir.")]
    public string BranchName { get; set; } = string.Empty;

    [Validate(
        ValidationRuleType.MaxLength,
        512,
        ErrorMessage = "A├ğ─▒klama en fazla 512 karakter olabilir.")]
    public string? Description { get; set; }

    [Validate(
        ValidationRuleType.MaxLength,
        512,
        ErrorMessage = "Adres en fazla 512 karakter olabilir.")]
    public string? Address { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public bool IsActive { get; set; }
}

// --- BranchIdRequest.cs ---
public sealed class BranchIdRequest
{
    [Validate(
        ValidationRuleType.GreaterThan,
        0,
        ErrorMessage = "Ge├ğerli bir ┼şube ID de─şeri g├Ânderilmelidir.")]
    public int BranchId { get; set; }
}

// --- CommunicationChannelTypeDto.cs ---
public sealed class CommunicationChannelTypeDto
{
    public int CommunicationChannelTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? IconUrl { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public sealed class CreateCommunicationChannelTypeDto
{
    [Validate(ValidationRuleType.Required, ErrorMessage = "T├╝r ad─▒ zorunludur.")]
    [Validate(ValidationRuleType.MaxLength, 128, ErrorMessage = "T├╝r ad─▒ en fazla 128 karakter olabilir.")]
    public string Name { get; set; } = string.Empty;

    [Validate(ValidationRuleType.Required, ErrorMessage = "T├╝r kodu zorunludur.")]
    [Validate(ValidationRuleType.MaxLength, 64, ErrorMessage = "T├╝r kodu en fazla 64 karakter olabilir.")]
    public string Code { get; set; } = string.Empty;

    [Validate(ValidationRuleType.MaxLength, 1024, ErrorMessage = "─░kon URL en fazla 1024 karakter olabilir.")]
    public string? IconUrl { get; set; }

    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class UpdateCommunicationChannelTypeDto
{
    [Validate(ValidationRuleType.GreaterThan, 0, ErrorMessage = "Ge├ğerli bir t├╝r ID de─şeri g├Ânderilmelidir.")]
    public int CommunicationChannelTypeId { get; set; }

    [Validate(ValidationRuleType.Required, ErrorMessage = "T├╝r ad─▒ zorunludur.")]
    [Validate(ValidationRuleType.MaxLength, 128, ErrorMessage = "T├╝r ad─▒ en fazla 128 karakter olabilir.")]
    public string Name { get; set; } = string.Empty;

    [Validate(ValidationRuleType.Required, ErrorMessage = "T├╝r kodu zorunludur.")]
    [Validate(ValidationRuleType.MaxLength, 64, ErrorMessage = "T├╝r kodu en fazla 64 karakter olabilir.")]
    public string Code { get; set; } = string.Empty;

    [Validate(ValidationRuleType.MaxLength, 1024, ErrorMessage = "─░kon URL en fazla 1024 karakter olabilir.")]
    public string? IconUrl { get; set; }

    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}

// --- CompanyDto.cs ---
public sealed class CompanyDto
{
    public int CompanyId { get; set; }

    public string CompanyCode { get; set; } = string.Empty;

    public string CompanyName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? LogoUrl { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

public sealed class CreateCompanyDto
{
    [Validate(
        ValidationRuleType.MaxLength,
        64,
        ErrorMessage = "┼Şirket kodu en fazla 64 karakter olabilir.")]
    public string? CompanyCode { get; set; }

    [Validate(
        ValidationRuleType.Required,
        ErrorMessage = "┼Şirket ad─▒ zorunludur.")]
    [Validate(
        ValidationRuleType.MaxLength,
        256,
        ErrorMessage = "┼Şirket ad─▒ en fazla 256 karakter olabilir.")]
    public string CompanyName { get; set; } = string.Empty;

    [Validate(
        ValidationRuleType.MaxLength,
        1024,
        ErrorMessage = "A├ğ─▒klama en fazla 1024 karakter olabilir.")]
    public string? Description { get; set; }

    [Validate(
        ValidationRuleType.MaxLength,
        1024,
        ErrorMessage = "Logo adresi en fazla 1024 karakter olabilir.")]
    public string? LogoUrl { get; set; }

    public bool IsActive { get; set; } = true;
}

public sealed class UpdateCompanyDto
{
    [Validate(
        ValidationRuleType.GreaterThan,
        0,
        ErrorMessage = "Ge├ğerli bir ┼şirket ID de─şeri g├Ânderilmelidir.")]
    public int CompanyId { get; set; }

    [Validate(
        ValidationRuleType.MaxLength,
        64,
        ErrorMessage = "┼Şirket kodu en fazla 64 karakter olabilir.")]
    public string? CompanyCode { get; set; }

    [Validate(
        ValidationRuleType.Required,
        ErrorMessage = "┼Şirket ad─▒ zorunludur.")]
    [Validate(
        ValidationRuleType.MaxLength,
        256,
        ErrorMessage = "┼Şirket ad─▒ en fazla 256 karakter olabilir.")]
    public string CompanyName { get; set; } = string.Empty;

    [Validate(
        ValidationRuleType.MaxLength,
        1024,
        ErrorMessage = "A├ğ─▒klama en fazla 1024 karakter olabilir.")]
    public string? Description { get; set; }

    [Validate(
        ValidationRuleType.MaxLength,
        1024,
        ErrorMessage = "Logo adresi en fazla 1024 karakter olabilir.")]
    public string? LogoUrl { get; set; }

    public bool IsActive { get; set; }
}

// --- CompanyIdRequest.cs ---
public sealed class CompanyIdRequest
{
    [Validate(
        ValidationRuleType.GreaterThan,
        0,
        ErrorMessage = "Ge├ğerli bir ┼şirket ID de─şeri g├Ânderilmelidir.")]
    public int CompanyId { get; set; }
}

// --- CorporateAppCategoryDto.cs ---
public sealed class CorporateAppCategoryDto
{
    public int CorporateAppCategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconUrl { get; set; }
    public string? ColorKey { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public sealed class CreateCorporateAppCategoryDto
{
    [Validate(ValidationRuleType.Required, ErrorMessage = "Kategori ad─▒ zorunludur.")]
    [Validate(ValidationRuleType.MaxLength, 128, ErrorMessage = "Kategori ad─▒ en fazla 128 karakter olabilir.")]
    public string Name { get; set; } = string.Empty;

    [Validate(ValidationRuleType.MaxLength, 512, ErrorMessage = "A├ğ─▒klama en fazla 512 karakter olabilir.")]
    public string? Description { get; set; }

    [Validate(ValidationRuleType.MaxLength, 1024, ErrorMessage = "─░kon URL en fazla 1024 karakter olabilir.")]
    public string? IconUrl { get; set; }

    [Validate(ValidationRuleType.MaxLength, 32, ErrorMessage = "Renk anahtar─▒ en fazla 32 karakter olabilir.")]
    public string? ColorKey { get; set; }

    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class UpdateCorporateAppCategoryDto
{
    [Validate(ValidationRuleType.GreaterThan, 0, ErrorMessage = "Ge├ğerli bir kategori ID de─şeri g├Ânderilmelidir.")]
    public int CorporateAppCategoryId { get; set; }

    [Validate(ValidationRuleType.Required, ErrorMessage = "Kategori ad─▒ zorunludur.")]
    [Validate(ValidationRuleType.MaxLength, 128, ErrorMessage = "Kategori ad─▒ en fazla 128 karakter olabilir.")]
    public string Name { get; set; } = string.Empty;

    [Validate(ValidationRuleType.MaxLength, 512, ErrorMessage = "A├ğ─▒klama en fazla 512 karakter olabilir.")]
    public string? Description { get; set; }

    [Validate(ValidationRuleType.MaxLength, 1024, ErrorMessage = "─░kon URL en fazla 1024 karakter olabilir.")]
    public string? IconUrl { get; set; }

    [Validate(ValidationRuleType.MaxLength, 32, ErrorMessage = "Renk anahtar─▒ en fazla 32 karakter olabilir.")]
    public string? ColorKey { get; set; }

    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}

// --- CurrentUserResponse.cs ---
/// <summary>
/// Authenticated kullan─▒c─▒n─▒n uygulama auth context ├Âzeti.
/// Kaynak: <c>ICurrentUser</c> / <c>ICompanyContext</c> (middleware ├ğ├Âz├╝mlemesi).
/// </summary>
public sealed record CurrentUserResponse(
    int UserId,
    string Username,
    string Email,
    string DisplayName,
    int? ActiveCompanyId,
    string? ActiveCompanyName,
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<string> Permissions,
    IReadOnlyCollection<CurrentUserCompanyResponse> Companies,
    bool HasAdminPanelAccess = false);

public sealed record CurrentUserCompanyResponse(
    int CompanyId,
    string CompanyName,
    IReadOnlyCollection<string> Roles);

// --- DashboardDto.cs ---
public sealed class DashboardSummaryDto
{
    public int Users { get; set; }
    public int ActiveCompanies { get; set; }
    public int Branches { get; set; }
    public int Departments { get; set; }
    public int Announcements { get; set; }
    public int UpcomingEvents { get; set; }
    public int ActiveServices { get; set; }
    public int MediaFiles { get; set; }
    public int Reservations { get; set; }
    public int PublishedWeeklyMenus { get; set; }
    public IReadOnlyList<DashboardEventsByMonthDto> EventsByMonth { get; set; } = [];
    public IReadOnlyList<DashboardUsersByCompanyDto> UsersByCompany { get; set; } = [];
}

public sealed class DashboardEventsByMonthDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int Count { get; set; }
}

public sealed class DashboardUsersByCompanyDto
{
    public int CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public int Count { get; set; }
}

// --- DepartmentDto.cs ---
public sealed class DepartmentDto
{
    public int DepartmentId { get; set; }

    public int? BranchId { get; set; }

    public int? CompanyId { get; set; }

    public int? ParentDepartmentId { get; set; }

    public string? DepartmentCode { get; set; }

    public string DepartmentName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

public sealed class CreateDepartmentDto
{
    [Validate(
        ValidationRuleType.GreaterThan,
        0,
        ErrorMessage = "┼Şube ID zorunludur.")]
    public int BranchId { get; set; }

    public int? ParentDepartmentId { get; set; }

    [Validate(
        ValidationRuleType.MaxLength,
        64,
        ErrorMessage = "Departman kodu en fazla 64 karakter olabilir.")]
    public string? DepartmentCode { get; set; }

    [Validate(
        ValidationRuleType.Required,
        ErrorMessage = "Departman ad─▒ zorunludur.")]
    [Validate(
        ValidationRuleType.MaxLength,
        256,
        ErrorMessage = "Departman ad─▒ en fazla 256 karakter olabilir.")]
    public string DepartmentName { get; set; } = string.Empty;

    [Validate(
        ValidationRuleType.MaxLength,
        512,
        ErrorMessage = "A├ğ─▒klama en fazla 512 karakter olabilir.")]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
}

public sealed class UpdateDepartmentDto
{
    [Validate(
        ValidationRuleType.GreaterThan,
        0,
        ErrorMessage = "Ge├ğerli bir departman ID de─şeri g├Ânderilmelidir.")]
    public int DepartmentId { get; set; }

    [Validate(
        ValidationRuleType.GreaterThan,
        0,
        ErrorMessage = "┼Şube ID zorunludur.")]
    public int BranchId { get; set; }

    public int? ParentDepartmentId { get; set; }

    [Validate(
        ValidationRuleType.MaxLength,
        64,
        ErrorMessage = "Departman kodu en fazla 64 karakter olabilir.")]
    public string? DepartmentCode { get; set; }

    [Validate(
        ValidationRuleType.Required,
        ErrorMessage = "Departman ad─▒ zorunludur.")]
    [Validate(
        ValidationRuleType.MaxLength,
        256,
        ErrorMessage = "Departman ad─▒ en fazla 256 karakter olabilir.")]
    public string DepartmentName { get; set; } = string.Empty;

    [Validate(
        ValidationRuleType.MaxLength,
        512,
        ErrorMessage = "A├ğ─▒klama en fazla 512 karakter olabilir.")]
    public string? Description { get; set; }

    public bool IsActive { get; set; }
}

// --- DishCategoryDto.cs ---
public sealed class DishCategoryDto
{
    public int DishCategoryId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

public sealed class CreateDishCategoryDto
{
    [Validate(ValidationRuleType.Required, ErrorMessage = "Kategori ad─▒ bo┼ş b─▒rak─▒lamaz.")]
    [Validate(ValidationRuleType.MaxLength, 128, ErrorMessage = "Kategori ad─▒ en fazla 128 karakter olabilir.")]
    public string Name { get; set; } = string.Empty;

    [Validate(ValidationRuleType.MaxLength, 64, ErrorMessage = "Kategori kodu en fazla 64 karakter olabilir.")]
    public string? Code { get; set; }

    [Validate(ValidationRuleType.MaxLength, 512, ErrorMessage = "A├ğ─▒klama en fazla 512 karakter olabilir.")]
    public string? Description { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;
}

public sealed class UpdateDishCategoryDto
{
    [Validate(ValidationRuleType.GreaterThan, 0, ErrorMessage = "Kategori kimli─şi s─▒f─▒rdan b├╝y├╝k olmal─▒d─▒r.")]
    public int DishCategoryId { get; set; }

    [Validate(ValidationRuleType.Required, ErrorMessage = "Kategori ad─▒ bo┼ş b─▒rak─▒lamaz.")]
    [Validate(ValidationRuleType.MaxLength, 128, ErrorMessage = "Kategori ad─▒ en fazla 128 karakter olabilir.")]
    public string Name { get; set; } = string.Empty;

    [Validate(ValidationRuleType.MaxLength, 64, ErrorMessage = "Kategori kodu en fazla 64 karakter olabilir.")]
    public string? Code { get; set; }

    [Validate(ValidationRuleType.MaxLength, 512, ErrorMessage = "A├ğ─▒klama en fazla 512 karakter olabilir.")]
    public string? Description { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; }
}

// --- MeetingRoomDto.cs ---
public sealed class MeetingRoomDto
{
    public int MeetingRoomId { get; set; }
    public int CompanyId { get; set; }
    public int? BranchId { get; set; }
    public int? DepartmentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Floor { get; set; }
    public int Capacity { get; set; }
    public string? LocationDescription { get; set; }
    public string? Features { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public sealed class CreateMeetingRoomDto
{
    [Validate(ValidationRuleType.GreaterThan, 0, ErrorMessage = "Ge├ğerli bir ┼şirket ID de─şeri g├Ânderilmelidir.")]
    public int CompanyId { get; set; }

    public int? BranchId { get; set; }
    public int? DepartmentId { get; set; }

    [Validate(ValidationRuleType.Required, ErrorMessage = "Oda ad─▒ zorunludur.")]
    [Validate(ValidationRuleType.MaxLength, 256, ErrorMessage = "Oda ad─▒ en fazla 256 karakter olabilir.")]
    public string Name { get; set; } = string.Empty;

    [Validate(ValidationRuleType.MaxLength, 64, ErrorMessage = "Oda kodu en fazla 64 karakter olabilir.")]
    public string? Code { get; set; }

    [Validate(ValidationRuleType.MaxLength, 64, ErrorMessage = "Kat bilgisi en fazla 64 karakter olabilir.")]
    public string? Floor { get; set; }

    [Validate(ValidationRuleType.GreaterThan, 0, ErrorMessage = "Kapasite en az 1 olmal─▒d─▒r.")]
    public int Capacity { get; set; }

    [Validate(ValidationRuleType.MaxLength, 512, ErrorMessage = "Konum a├ğ─▒klamas─▒ en fazla 512 karakter olabilir.")]
    public string? LocationDescription { get; set; }

    [Validate(ValidationRuleType.MaxLength, 512, ErrorMessage = "├ûzellikler en fazla 512 karakter olabilir.")]
    public string? Features { get; set; }
}

public sealed class UpdateMeetingRoomDto
{
    [Validate(ValidationRuleType.GreaterThan, 0, ErrorMessage = "Ge├ğerli bir oda ID de─şeri g├Ânderilmelidir.")]
    public int MeetingRoomId { get; set; }

    [Validate(ValidationRuleType.GreaterThan, 0, ErrorMessage = "Ge├ğerli bir ┼şirket ID de─şeri g├Ânderilmelidir.")]
    public int CompanyId { get; set; }

    public int? BranchId { get; set; }
    public int? DepartmentId { get; set; }

    [Validate(ValidationRuleType.Required, ErrorMessage = "Oda ad─▒ zorunludur.")]
    [Validate(ValidationRuleType.MaxLength, 256, ErrorMessage = "Oda ad─▒ en fazla 256 karakter olabilir.")]
    public string Name { get; set; } = string.Empty;

    [Validate(ValidationRuleType.MaxLength, 64, ErrorMessage = "Oda kodu en fazla 64 karakter olabilir.")]
    public string? Code { get; set; }

    [Validate(ValidationRuleType.MaxLength, 64, ErrorMessage = "Kat bilgisi en fazla 64 karakter olabilir.")]
    public string? Floor { get; set; }

    [Validate(ValidationRuleType.GreaterThan, 0, ErrorMessage = "Kapasite en az 1 olmal─▒d─▒r.")]
    public int Capacity { get; set; }

    [Validate(ValidationRuleType.MaxLength, 512, ErrorMessage = "Konum a├ğ─▒klamas─▒ en fazla 512 karakter olabilir.")]
    public string? LocationDescription { get; set; }

    [Validate(ValidationRuleType.MaxLength, 512, ErrorMessage = "├ûzellikler en fazla 512 karakter olabilir.")]
    public string? Features { get; set; }

    public bool IsActive { get; set; }
}

// --- OrganizationScopeFieldsDto.cs ---
public class OrganizationScopeFieldsDto
{
    public string CompanyScope { get; set; } = "All";

    public int? CompanyId { get; set; }

    public string BranchScope { get; set; } = "All";

    public int? BranchId { get; set; }

    public string DepartmentScope { get; set; } = "All";

    public int? DepartmentId { get; set; }
}

// --- RoleDto.cs ---
public sealed class RoleDto
{
    public int RoleId { get; set; }

    public string RoleName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public IReadOnlyList<string> PermissionCodes { get; set; } = [];
}

public sealed class RoleListItemDto
{
    public int RoleId { get; set; }

    public string RoleName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; }
}

public sealed class CreateRoleDto
{
    [Validate(
        ValidationRuleType.Required,
        ErrorMessage = "Rol ad─▒ zorunludur.")]
    [Validate(
        ValidationRuleType.MaxLength,
        64,
        ErrorMessage = "Rol ad─▒ en fazla 64 karakter olabilir.")]
    public string RoleName { get; set; } = string.Empty;

    [Validate(
        ValidationRuleType.MaxLength,
        256,
        ErrorMessage = "A├ğ─▒klama en fazla 256 karakter olabilir.")]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
}

public sealed class UpdateRoleDto
{
    [Validate(
        ValidationRuleType.GreaterThan,
        0,
        ErrorMessage = "Ge├ğerli bir rol ID de─şeri g├Ânderilmelidir.")]
    public int RoleId { get; set; }

    [Validate(
        ValidationRuleType.Required,
        ErrorMessage = "Rol ad─▒ zorunludur.")]
    [Validate(
        ValidationRuleType.MaxLength,
        64,
        ErrorMessage = "Rol ad─▒ en fazla 64 karakter olabilir.")]
    public string RoleName { get; set; } = string.Empty;

    [Validate(
        ValidationRuleType.MaxLength,
        256,
        ErrorMessage = "A├ğ─▒klama en fazla 256 karakter olabilir.")]
    public string? Description { get; set; }

    public bool IsActive { get; set; }
}

public sealed class UpdateRolePermissionsDto
{
    public int[] PermissionIds { get; set; } = [];
}

public sealed class PermissionDto
{
    public int PermissionId { get; set; }

    public string PermissionCode { get; set; } = string.Empty;

    public string? Description { get; set; }
}

// --- ServiceLocationDto.cs ---
public sealed class ServiceLocationDto
{
    public long ServiceLocationId { get; set; }
    public int? CompanyId { get; set; }
    public int? BranchId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? ServiceLocationTypeId { get; set; }
    public string? TypeName { get; set; }
    public string LocationType { get; set; } = string.Empty;
    public string? Address { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public sealed class CreateServiceLocationDto
{
    public int? CompanyId { get; set; }
    public int? BranchId { get; set; }

    [Validate(ValidationRuleType.Required, ErrorMessage = "Konum ad─▒ zorunludur.")]
    [Validate(ValidationRuleType.MaxLength, 256, ErrorMessage = "Konum ad─▒ en fazla 256 karakter olabilir.")]
    public string Name { get; set; } = string.Empty;

    public int? ServiceLocationTypeId { get; set; }

    [Validate(ValidationRuleType.MaxLength, 32, ErrorMessage = "Konum t├╝r├╝ en fazla 32 karakter olabilir.")]
    public string? LocationType { get; set; }

    [Validate(ValidationRuleType.MaxLength, 512, ErrorMessage = "Adres en fazla 512 karakter olabilir.")]
    public string? Address { get; set; }

    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
}

public sealed class UpdateServiceLocationDto
{
    [Validate(ValidationRuleType.GreaterThan, 0, ErrorMessage = "Ge├ğerli bir konum ID de─şeri g├Ânderilmelidir.")]
    public long ServiceLocationId { get; set; }

    public int? CompanyId { get; set; }
    public int? BranchId { get; set; }

    [Validate(ValidationRuleType.Required, ErrorMessage = "Konum ad─▒ zorunludur.")]
    [Validate(ValidationRuleType.MaxLength, 256, ErrorMessage = "Konum ad─▒ en fazla 256 karakter olabilir.")]
    public string Name { get; set; } = string.Empty;

    public int? ServiceLocationTypeId { get; set; }

    [Validate(ValidationRuleType.MaxLength, 32, ErrorMessage = "Konum t├╝r├╝ en fazla 32 karakter olabilir.")]
    public string? LocationType { get; set; }

    [Validate(ValidationRuleType.MaxLength, 512, ErrorMessage = "Adres en fazla 512 karakter olabilir.")]
    public string? Address { get; set; }

    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public bool IsActive { get; set; }
}

public sealed class ServiceRouteStopDto
{
    public long ServiceRouteStopId { get; set; }
    public long ServiceRouteId { get; set; }
    public long ServiceLocationId { get; set; }
    public int StopOrder { get; set; }
    public string? ArrivalTime { get; set; }
    public string? DepartureTime { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
}

public sealed class ServiceRouteStopInputDto
{
    public long? ServiceRouteStopId { get; set; }
    public long ServiceLocationId { get; set; }
    public int StopOrder { get; set; }
    public string? ArrivalTime { get; set; }
    public string? DepartureTime { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
}

// --- ServiceLocationTypeDto.cs ---
public sealed class ServiceLocationTypeDto
{
    public int ServiceLocationTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconUrl { get; set; }
    public string? ColorKey { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public sealed class CreateServiceLocationTypeDto
{
    [Validate(ValidationRuleType.Required, ErrorMessage = "T├╝r ad─▒ zorunludur.")]
    [Validate(ValidationRuleType.MaxLength, 64, ErrorMessage = "T├╝r ad─▒ en fazla 64 karakter olabilir.")]
    public string Name { get; set; } = string.Empty;

    [Validate(ValidationRuleType.MaxLength, 512, ErrorMessage = "A├ğ─▒klama en fazla 512 karakter olabilir.")]
    public string? Description { get; set; }

    [Validate(ValidationRuleType.MaxLength, 1024, ErrorMessage = "─░kon URL en fazla 1024 karakter olabilir.")]
    public string? IconUrl { get; set; }

    [Validate(ValidationRuleType.MaxLength, 32, ErrorMessage = "Renk anahtar─▒ en fazla 32 karakter olabilir.")]
    public string? ColorKey { get; set; }

    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class UpdateServiceLocationTypeDto
{
    [Validate(ValidationRuleType.GreaterThan, 0, ErrorMessage = "Ge├ğerli bir t├╝r ID de─şeri g├Ânderilmelidir.")]
    public int ServiceLocationTypeId { get; set; }

    [Validate(ValidationRuleType.Required, ErrorMessage = "T├╝r ad─▒ zorunludur.")]
    [Validate(ValidationRuleType.MaxLength, 64, ErrorMessage = "T├╝r ad─▒ en fazla 64 karakter olabilir.")]
    public string Name { get; set; } = string.Empty;

    [Validate(ValidationRuleType.MaxLength, 512, ErrorMessage = "A├ğ─▒klama en fazla 512 karakter olabilir.")]
    public string? Description { get; set; }

    [Validate(ValidationRuleType.MaxLength, 1024, ErrorMessage = "─░kon URL en fazla 1024 karakter olabilir.")]
    public string? IconUrl { get; set; }

    [Validate(ValidationRuleType.MaxLength, 32, ErrorMessage = "Renk anahtar─▒ en fazla 32 karakter olabilir.")]
    public string? ColorKey { get; set; }

    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}

// --- SocialActivityDto.cs ---
public sealed class SocialActivityDto : OrganizationScopeFieldsDto
{
    public long SocialActivityId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ActivityType { get; set; } = string.Empty;
    public string? Location { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public string? ImageUrl { get; set; }
    public string Status { get; set; } = "Draft";
    public bool IsActive { get; set; }
    public int CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? CompanyName { get; set; }
    public string? BranchName { get; set; }
    public string? DepartmentName { get; set; }
    public string ScopeLabel { get; set; } = string.Empty;
}

public sealed class CreateSocialActivityDto : OrganizationScopeFieldsDto
{
    [Validate(ValidationRuleType.Required, ErrorMessage = "Ba┼şl─▒k zorunludur.")]
    [Validate(ValidationRuleType.MaxLength, 512, ErrorMessage = "Ba┼şl─▒k en fazla 512 karakter olabilir.")]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Validate(ValidationRuleType.Required, ErrorMessage = "Aktivite t├╝r├╝ zorunludur.")]
    [Validate(ValidationRuleType.MaxLength, 64, ErrorMessage = "Aktivite t├╝r├╝ en fazla 64 karakter olabilir.")]
    public string ActivityType { get; set; } = string.Empty;

    [Validate(ValidationRuleType.MaxLength, 256, ErrorMessage = "Konum en fazla 256 karakter olabilir.")]
    public string? Location { get; set; }

    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }

    [Validate(ValidationRuleType.MaxLength, 1024, ErrorMessage = "G├Ârsel URL en fazla 1024 karakter olabilir.")]
    public string? ImageUrl { get; set; }

    public string Status { get; set; } = "Draft";
}

public sealed class UpdateSocialActivityDto : OrganizationScopeFieldsDto
{
    [Validate(ValidationRuleType.GreaterThan, 0, ErrorMessage = "Ge├ğerli bir aktivite ID de─şeri g├Ânderilmelidir.")]
    public long SocialActivityId { get; set; }

    [Validate(ValidationRuleType.Required, ErrorMessage = "Ba┼şl─▒k zorunludur.")]
    [Validate(ValidationRuleType.MaxLength, 512, ErrorMessage = "Ba┼şl─▒k en fazla 512 karakter olabilir.")]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Validate(ValidationRuleType.Required, ErrorMessage = "Aktivite t├╝r├╝ zorunludur.")]
    [Validate(ValidationRuleType.MaxLength, 64, ErrorMessage = "Aktivite t├╝r├╝ en fazla 64 karakter olabilir.")]
    public string ActivityType { get; set; } = string.Empty;

    [Validate(ValidationRuleType.MaxLength, 256, ErrorMessage = "Konum en fazla 256 karakter olabilir.")]
    public string? Location { get; set; }

    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }

    [Validate(ValidationRuleType.MaxLength, 1024, ErrorMessage = "G├Ârsel URL en fazla 1024 karakter olabilir.")]
    public string? ImageUrl { get; set; }

    public string Status { get; set; } = "Draft";
    public bool IsActive { get; set; }
}

// --- UserLookupDto.cs ---
public sealed class UserLookupDto
{
    public int UserId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
}

// --- WeeklyMenuDto.cs ---
public sealed class WeeklyMenuDto
{
    public long MenuId { get; set; }

    public int CompanyId { get; set; }

    public string MenuCode { get; set; } = string.Empty;

    public int Year { get; set; }

    public int Month { get; set; }

    public int WeekOfMonth { get; set; }

    public DateOnly PeriodStartDate { get; set; }

    public DateOnly PeriodEndDate { get; set; }

    public string? Title { get; set; }

    public bool IsPublished { get; set; }

    public DateTime? PublishedAt { get; set; }

    public bool IsActive { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public IReadOnlyList<WeeklyMenuItemDto> Items { get; set; } = [];
}

public sealed class CreateWeeklyMenuDto
{
    [Validate(ValidationRuleType.GreaterThan, 0, ErrorMessage = "┼Şirket kimli─şi s─▒f─▒rdan b├╝y├╝k olmal─▒d─▒r.")]
    public int CompanyId { get; set; }

    [Validate(ValidationRuleType.GreaterThan, 0, ErrorMessage = "Y─▒l s─▒f─▒rdan b├╝y├╝k olmal─▒d─▒r.")]
    public int Year { get; set; }

    [Validate(ValidationRuleType.GreaterThanOrEqual, 1, ErrorMessage = "Ay 1 ile 12 aras─▒nda olmal─▒d─▒r.")]
    [Validate(ValidationRuleType.LessThanOrEqual, 12, ErrorMessage = "Ay 1 ile 12 aras─▒nda olmal─▒d─▒r.")]
    public int Month { get; set; }

    [Validate(ValidationRuleType.GreaterThanOrEqual, 1, ErrorMessage = "Hafta 1 ile 5 aras─▒nda olmal─▒d─▒r.")]
    [Validate(ValidationRuleType.LessThanOrEqual, 5, ErrorMessage = "Hafta 1 ile 5 aras─▒nda olmal─▒d─▒r.")]
    public int WeekOfMonth { get; set; }

    [Validate(ValidationRuleType.MaxLength, 256, ErrorMessage = "Ba┼şl─▒k en fazla 256 karakter olabilir.")]
    public string? Title { get; set; }
}

public sealed class UpdateWeeklyMenuDto
{
    [Validate(ValidationRuleType.GreaterThan, 0, ErrorMessage = "Men├╝ kimli─şi s─▒f─▒rdan b├╝y├╝k olmal─▒d─▒r.")]
    public long MenuId { get; set; }

    [Validate(ValidationRuleType.MaxLength, 256, ErrorMessage = "Ba┼şl─▒k en fazla 256 karakter olabilir.")]
    public string? Title { get; set; }
}

public sealed class WeeklyMenuItemDto
{
    public long MenuItemId { get; set; }

    public long MenuId { get; set; }

    public DateOnly MenuDate { get; set; }

    public int DishCategoryId { get; set; }

    public string? DishCategoryName { get; set; }

    public int DishId { get; set; }

    public string? DishName { get; set; }

    public int SortOrder { get; set; }

    public string? Notes { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

public sealed class CreateWeeklyMenuItemDto
{
    [Validate(ValidationRuleType.GreaterThan, 0, ErrorMessage = "Men├╝ kimli─şi s─▒f─▒rdan b├╝y├╝k olmal─▒d─▒r.")]
    public long MenuId { get; set; }

    public DateOnly MenuDate { get; set; }

    [Validate(ValidationRuleType.GreaterThan, 0, ErrorMessage = "Yemek kategorisi se├ğilmelidir.")]
    public int DishCategoryId { get; set; }

    [Validate(ValidationRuleType.GreaterThan, 0, ErrorMessage = "Yemek se├ğilmelidir.")]
    public int DishId { get; set; }

    public int SortOrder { get; set; }

    [Validate(ValidationRuleType.MaxLength, 512, ErrorMessage = "Not en fazla 512 karakter olabilir.")]
    public string? Notes { get; set; }
}

public sealed class UpdateWeeklyMenuItemDto
{
    [Validate(ValidationRuleType.GreaterThan, 0, ErrorMessage = "Men├╝ kimli─şi s─▒f─▒rdan b├╝y├╝k olmal─▒d─▒r.")]
    public long MenuId { get; set; }

    [Validate(ValidationRuleType.GreaterThan, 0, ErrorMessage = "Men├╝ ├Â─şesi kimli─şi s─▒f─▒rdan b├╝y├╝k olmal─▒d─▒r.")]
    public long MenuItemId { get; set; }

    public DateOnly MenuDate { get; set; }

    [Validate(ValidationRuleType.GreaterThan, 0, ErrorMessage = "Yemek kategorisi se├ğilmelidir.")]
    public int DishCategoryId { get; set; }

    [Validate(ValidationRuleType.GreaterThan, 0, ErrorMessage = "Yemek se├ğilmelidir.")]
    public int DishId { get; set; }

    public int SortOrder { get; set; }

    [Validate(ValidationRuleType.MaxLength, 512, ErrorMessage = "Not en fazla 512 karakter olabilir.")]
    public string? Notes { get; set; }
}

public sealed class WeeklyMenuItemRouteRequest
{
    [Validate(ValidationRuleType.GreaterThan, 0, ErrorMessage = "Men├╝ kimli─şi s─▒f─▒rdan b├╝y├╝k olmal─▒d─▒r.")]
    public long MenuId { get; set; }

    [Validate(ValidationRuleType.GreaterThan, 0, ErrorMessage = "Men├╝ ├Â─şesi kimli─şi s─▒f─▒rdan b├╝y├╝k olmal─▒d─▒r.")]
    public long MenuItemId { get; set; }
}

public sealed class WeeklyMenuRouteRequest
{
    [Validate(ValidationRuleType.GreaterThan, 0, ErrorMessage = "Men├╝ kimli─şi s─▒f─▒rdan b├╝y├╝k olmal─▒d─▒r.")]
    public long MenuId { get; set; }
}

