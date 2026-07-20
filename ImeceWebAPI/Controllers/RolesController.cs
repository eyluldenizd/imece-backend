using Application.DTOs;
using Application.Services;
using Core.Authorization;
using ImeceWebAPI.Controllers.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ImeceWebAPI.Controllers;

[ApiController]
[Route("api/roles/")]
[Authorize(Policy = ImecePolicies.RequireRegisteredUser)]
public sealed class RolesController : ApiControllerBase
{
    private readonly RoleService _roleService;

    public RolesController(RoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpGet("")]
    public Task<IActionResult> GetAll(CancellationToken cancellationToken) =>
        ExecuteAsync(_roleService.GetAllAsync, cancellationToken);

    [HttpGet("{id:int}")]
    public Task<IActionResult> GetById(int id, CancellationToken cancellationToken) =>
        ExecuteAsync(new IdRequest { Id = id }, _roleService.GetByIdAsync, cancellationToken);

    [HttpPost("")]
    [Authorize(Policy = ImecePolicies.RequireGlobalAdmin)]
    public Task<IActionResult> Create(
        [FromBody] CreateRoleDto request,
        CancellationToken cancellationToken) =>
        ExecuteAsync(request, _roleService.CreateAsync, cancellationToken);

    [HttpPut("{id:int}")]
    [Authorize(Policy = ImecePolicies.RequireGlobalAdmin)]
    public Task<IActionResult> Update(
        int id,
        [FromBody] UpdateRoleDto request,
        CancellationToken cancellationToken)
    {
        request.RoleId = id;
        return ExecuteAsync(request, _roleService.UpdateAsync, cancellationToken);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = ImecePolicies.RequireGlobalAdmin)]
    public Task<IActionResult> Delete(int id, CancellationToken cancellationToken) =>
        ExecuteAsync(new IdRequest { Id = id }, _roleService.DeleteAsync, cancellationToken);

    [HttpPut("{id:int}/permissions")]
    [Authorize(Policy = ImecePolicies.RequireGlobalAdmin)]
    public Task<IActionResult> UpdatePermissions(
        int id,
        [FromBody] UpdateRolePermissionsDto request,
        CancellationToken cancellationToken)
    {
        var updateRequest = new UpdateRolePermissionsRequest
        {
            RoleId = id,
            PermissionIds = request.PermissionIds
        };

        return ExecuteAsync(updateRequest, _roleService.UpdatePermissionsAsync, cancellationToken);
    }
}
