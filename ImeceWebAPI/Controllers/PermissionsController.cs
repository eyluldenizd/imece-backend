using Application.Services;
using Core.Authorization;
using ImeceWebAPI.Controllers.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ImeceWebAPI.Controllers;

[ApiController]
[Route("api/permissions/")]
[Authorize(Policy = ImecePolicies.RequireRegisteredUser)]
public sealed class PermissionsController : ApiControllerBase
{
    private readonly PermissionService _permissionService;

    public PermissionsController(PermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    [HttpGet("")]
    public Task<IActionResult> GetAll(CancellationToken cancellationToken) =>
        ExecuteAsync(_permissionService.GetAllAsync, cancellationToken);
}
