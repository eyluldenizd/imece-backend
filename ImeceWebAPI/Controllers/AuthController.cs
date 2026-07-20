using Application.DTOs;
using Application.Services;
using Core.Authorization;
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

    public AuthController(
        IAuthenticationService authenticationService,
        ICurrentUser currentUser,
        ICompanyContext companyContext)
    {
        _authenticationService = authenticationService;
        _currentUser = currentUser;
        _companyContext = companyContext;
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
    public ActionResult<CurrentUserResponse> GetMe()
    {
        return Ok(BuildCurrentUserResponse());
    }

    [HttpPost("logout")]
    [Authorize]
    public IActionResult Logout()
    {
        // JWT stateless: istemci token'ı atar.
        return Ok(new { message = "Çıkış yapıldı." });
    }

    private CurrentUserResponse BuildCurrentUserResponse()
    {
        var companies = _currentUser.CompanyMemberships
            .Select(membership => new CurrentUserCompanyResponse(
                membership.CompanyId,
                membership.CompanyName ?? string.Empty,
                membership.Roles))
            .ToArray();

        return new CurrentUserResponse(
            UserId: _currentUser.GetRequiredUserId(),
            Username: _currentUser.Username ?? string.Empty,
            Email: _currentUser.Email ?? string.Empty,
            DisplayName: _currentUser.DisplayName ?? string.Empty,
            ActiveCompanyId: _companyContext.CurrentCompanyId,
            ActiveCompanyName: _companyContext.CompanyName,
            Roles: _currentUser.Roles,
            Permissions: _currentUser.Permissions,
            Companies: companies,
            HasAdminPanelAccess: _currentUser.HasPermission(Permissions.AdminPanelAccess));
    }
}
