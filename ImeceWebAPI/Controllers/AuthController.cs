using Application.DTOs;
using Application.Services;
using Core.Authorization;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ImeceWebAPI.Controllers;

/// <summary>
/// LocalJwt login/logout ve mevcut auth context endpoint'leri.
/// Login JWT üretir; me/logout çözümlenmiş kullanıcıyı okur.
/// </summary>
[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ICurrentUser _currentUser;
    private readonly ICompanyContext _companyContext;
    private readonly ICompanyAuthorizationService _companyAuthorization;
    private readonly RoleRepository _roleRepository;

    public AuthController(
        IAuthenticationService authenticationService,
        ICurrentUser currentUser,
        ICompanyContext companyContext,
        ICompanyAuthorizationService companyAuthorization,
        RoleRepository roleRepository)
    {
        _authenticationService = authenticationService;
        _currentUser = currentUser;
        _companyContext = companyContext;
        _companyAuthorization = companyAuthorization;
        _roleRepository = roleRepository;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponseDto>> Login(
        [FromBody] LoginRequestDto request,
        CancellationToken cancellationToken)
    {
        var response = await _authenticationService.LoginAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpGet("me")]
    [Authorize(Policy = ImecePolicies.RequireRegisteredUser)]
    public async Task<ActionResult<CurrentUserResponse>> GetMe(CancellationToken cancellationToken)
    {
        var roleDetails = await ResolveRoleDetailsAsync(cancellationToken);

        var canAccessAll = _companyAuthorization.CanAccessAllCompanies;
        var organizationScope = OrganizationScopeCodes.ToCode(
            OrganizationScopeCodes.FromHasGlobalAccess(canAccessAll));

        // Global users: companies=[] by contract (use accessible-companies for full list).
        var companies = canAccessAll
            ? Array.Empty<CurrentUserCompanyResponse>()
            : _currentUser.CompanyMemberships
                .OrderBy(m => m.CompanyName, StringComparer.OrdinalIgnoreCase)
                .Select(membership => new CurrentUserCompanyResponse(
                    membership.CompanyId,
                    membership.CompanyName ?? string.Empty,
                    membership.Roles))
                .ToArray();

        var roles = _currentUser.Roles
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(r => r, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var permissions = _currentUser.Permissions
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return Ok(new CurrentUserResponse(
            UserId: _currentUser.GetRequiredUserId(),
            Username: _currentUser.Username ?? string.Empty,
            Email: _currentUser.Email ?? string.Empty,
            DisplayName: _currentUser.DisplayName ?? string.Empty,
            ActiveCompanyId: _companyContext.CurrentCompanyId,
            ActiveCompanyName: _companyContext.CompanyName,
            Roles: roles,
            Permissions: permissions,
            Companies: companies,
            HasAdminPanelAccess: PermissionSatisfaction.Satisfies(
                permissions,
                Permissions.AdminPanelAccess),
            OrganizationScope: organizationScope,
            RoleDetails: roleDetails));
    }

    [HttpPost("logout")]
    [Authorize]
    public IActionResult Logout()
    {
        // JWT stateless: istemci token'ı atar.
        return Ok(new { message = "Çıkış yapıldı." });
    }

    private async Task<IReadOnlyCollection<CurrentUserRoleResponse>> ResolveRoleDetailsAsync(
        CancellationToken cancellationToken)
    {
        if (_currentUser.Roles.Count == 0)
        {
            return [];
        }

        var all = await _roleRepository.GetAllAsync(cancellationToken);
        return all
            .Where(role =>
                role.IsActive
                && _currentUser.Roles.Contains(role.RoleName, StringComparer.OrdinalIgnoreCase))
            .OrderBy(role => role.RoleName, StringComparer.OrdinalIgnoreCase)
            .Select(role => new CurrentUserRoleResponse(role.RoleId, role.RoleName, role.RoleName))
            .ToArray();
    }
}
