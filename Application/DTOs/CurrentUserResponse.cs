namespace Application.DTOs;

/// <summary>
/// Authenticated kullanıcının uygulama auth context özeti.
/// Kaynak: <c>ICurrentUser</c> / <c>ICompanyContext</c> (middleware çözümlemesi).
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
