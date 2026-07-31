using Application.DTOs;
using Application.Services;
using Core.Authorization;
using ImeceWebAPI.Controllers.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ImeceWebAPI.Controllers;

/// <summary>
/// Oturum açmış kullanıcının kendi profili ve şifre işlemleri.
/// UserId her zaman claim/context'ten alınır; body'den kabul edilmez.
/// </summary>
[ApiController]
[Route("api/profile")]
[Authorize(Policy = ImecePolicies.RequireRegisteredUser)]
public sealed class ProfileController : ApiControllerBase
{
    private readonly ProfileService _profileService;

    public ProfileController(ProfileService profileService)
    {
        _profileService = profileService;
    }

    [HttpGet("")]
    public Task<IActionResult> GetCurrent(CancellationToken cancellationToken) =>
        ExecuteAsync(_profileService.GetCurrentProfileAsync, cancellationToken);

    [HttpPut("password")]
    public Task<IActionResult> ChangePassword(
        [FromBody] ChangeMyPasswordDto request,
        CancellationToken cancellationToken) =>
        ExecuteAsync(request, _profileService.ChangeCurrentPasswordAsync, cancellationToken);
}
