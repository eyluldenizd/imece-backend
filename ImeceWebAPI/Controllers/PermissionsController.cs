using Application.DTOs;
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
    [Authorize(Policy = ImecePolicies.RequirePermissionsView)]
    public Task<IActionResult> GetAll(CancellationToken cancellationToken) =>
        ExecuteAsync(_permissionService.GetAllAsync, cancellationToken);

    [HttpGet("{id:int}")]
    [Authorize(Policy = ImecePolicies.RequirePermissionsView)]
    public Task<IActionResult> GetById(int id, CancellationToken cancellationToken) =>
        ExecuteAsync(new IdRequest { Id = id }, _permissionService.GetByIdAsync, cancellationToken);

    [HttpPost("")]
    [Authorize(Policy = ImecePolicies.RequirePermissionsManage)]
    public Task<IActionResult> Create(
        [FromBody] CreatePermissionDto request,
        CancellationToken cancellationToken) =>
        ExecuteAsync(request, _permissionService.CreateAsync, cancellationToken);

    [HttpPut("{id:int}")]
    [Authorize(Policy = ImecePolicies.RequirePermissionsManage)]
    public Task<IActionResult> Update(
        int id,
        [FromBody] UpdatePermissionDto request,
        CancellationToken cancellationToken)
    {
        request.PermissionId = id;
        return ExecuteAsync(request, _permissionService.UpdateAsync, cancellationToken);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = ImecePolicies.RequirePermissionsManage)]
    public Task<IActionResult> Delete(int id, CancellationToken cancellationToken) =>
        ExecuteAsync(new IdRequest { Id = id }, _permissionService.DeleteAsync, cancellationToken);
}
