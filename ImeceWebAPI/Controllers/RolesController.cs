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
    [Authorize(Policy = ImecePolicies.RequireRolesView)]
    public Task<IActionResult> GetAll(
        [FromQuery] ContentListQueryDto query,
        CancellationToken cancellationToken) =>
        ExecuteAsync(
            token => _roleService.GetAllAsync(query, token),
            cancellationToken);

    [HttpGet("{id:int}")]
    [Authorize(Policy = ImecePolicies.RequireRolesView)]
    public Task<IActionResult> GetById(int id, CancellationToken cancellationToken) =>
        ExecuteAsync(new IdRequest { Id = id }, _roleService.GetByIdAsync, cancellationToken);

    [HttpPost("")]
    [Authorize(Policy = ImecePolicies.RequireRolesManage)]
    public Task<IActionResult> Create(
        [FromBody] CreateRoleDto request,
        CancellationToken cancellationToken) =>
        ExecuteAsync(request, _roleService.CreateAsync, cancellationToken);

    [HttpPut("{id:int}")]
    [Authorize(Policy = ImecePolicies.RequireRolesManage)]
    public Task<IActionResult> Update(
        int id,
        [FromBody] UpdateRoleDto request,
        CancellationToken cancellationToken)
    {
        request.RoleId = id;
        return ExecuteAsync(request, _roleService.UpdateAsync, cancellationToken);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = ImecePolicies.RequireRolesManage)]
    public Task<IActionResult> Delete(int id, CancellationToken cancellationToken) =>
        ExecuteAsync(new IdRequest { Id = id }, _roleService.DeleteAsync, cancellationToken);

    [HttpPut("{id:int}/permissions")]
    [Authorize(Policy = ImecePolicies.RequireRolesManage)]
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
