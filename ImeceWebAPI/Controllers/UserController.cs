using Application.DTOs;
using Application.Services;
using Core.Authorization;
using ImeceWebAPI.Controllers.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ImeceWebAPI.Controllers;

[ApiController]
[Route("api/users/")]
[Authorize(Policy = ImecePolicies.RequireRegisteredUser)]
public sealed class UsersController : ApiControllerBase
{
    private readonly UserService _userService;
    private readonly UserAccessService _userAccessService;

    public UsersController(
        UserService userService,
        UserAccessService userAccessService)
    {
        _userService = userService;
        _userAccessService = userAccessService;
    }

    [HttpGet("get-all-users")]
    [Authorize(Policy = ImecePolicies.RequireUsersView)]
    public Task<IActionResult> GetAll(
        [FromQuery] ContentListQueryDto query,
        CancellationToken cancellationToken)
    {
        return ExecuteAsync(
            token => _userService.GetAllAsync(query, token),
            cancellationToken);
    }

    /// <summary>
    /// Company-scoped users for the current principal, with server-side pagination.
    /// Prefer this over get-all-users for admin list pages.
    /// </summary>
    [HttpGet("get-authorized-users")]
    [Authorize(Policy = ImecePolicies.RequireUsersView)]
    public Task<IActionResult> GetAuthorized(
        [FromQuery] ContentListQueryDto query,
        CancellationToken cancellationToken)
    {
        return ExecuteAsync(
            token => _userService.GetAuthorizedPagedAsync(query, token),
            cancellationToken);
    }

    [HttpGet("get-active-users")]
    [Authorize(Policy = ImecePolicies.RequireUsersView)]
    public Task<IActionResult> GetActive(
        CancellationToken cancellationToken)
    {
        return ExecuteAsync(
            _userService.GetActiveAsync,
            cancellationToken);
    }

    [HttpGet("lookup")]
    [Authorize(Policy = ImecePolicies.RequireUsersView)]
    public Task<IActionResult> Lookup(
        CancellationToken cancellationToken)
    {
        return ExecuteAsync(
            _userService.GetLookupAsync,
            cancellationToken);
    }

    [HttpGet("get-user-by-id/{id:int}")]
    [Authorize(Policy = ImecePolicies.RequireUsersView)]
    public Task<IActionResult> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var request = new IdRequest
        {
            Id = id
        };

        return ExecuteAsync(
            request,
            _userService.GetByIdAsync,
            cancellationToken);
    }

    [HttpGet("search-users")]
    [Authorize(Policy = ImecePolicies.RequireUsersView)]
    public Task<IActionResult> Search(
        [FromQuery] string searchText, //ortak dto herş ey için kullanılabilir
        CancellationToken cancellationToken)
    {
        return ExecuteAsync(
            token => _userService.SearchAsync(
                searchText,
                token),
            cancellationToken);
    }

    [HttpPost("create-user")]
    [Authorize(Policy = ImecePolicies.RequireUsersManage)]
    public Task<IActionResult> Create(
        [FromBody] CreateUserDto request,
        CancellationToken cancellationToken)
    {
        return ExecuteAsync(
            request,
            _userService.CreateAsync,
            cancellationToken);
    }

    [HttpPut("update-user-by-id/{id:int}")]
    [Authorize(Policy = ImecePolicies.RequireUsersManage)]
    public Task<IActionResult> Update(
        int id,
        [FromBody] UpdateUserDto request,
        CancellationToken cancellationToken)
    {
        request.UserId = id;

        return ExecuteAsync(
            request,
            _userService.UpdateAsync,
            cancellationToken);
    }

    [HttpGet("{id:int}/roles")]
    [Authorize(Policy = ImecePolicies.RequireUsersView)]
    public Task<IActionResult> GetRoles(
        int id,
        CancellationToken cancellationToken) =>
        ExecuteAsync(
            new IdRequest { Id = id },
            _userAccessService.GetUserRolesAsync,
            cancellationToken);

    [HttpPut("{id:int}/roles")]
    [Authorize(Policy = ImecePolicies.RequireUsersManage)]
    public Task<IActionResult> UpdateRoles(
        int id,
        [FromBody] UpdateUserRolesDto request,
        CancellationToken cancellationToken) =>
        ExecuteAsync(
            new UpdateUserRolesRequest
            {
                UserId = id,
                RoleIds = request.RoleIds
            },
            _userAccessService.UpdateUserRolesAsync,
            cancellationToken);

    [HttpGet("{id:int}/companies")]
    [Authorize(Policy = ImecePolicies.RequireUsersView)]
    public Task<IActionResult> GetCompanies(
        int id,
        CancellationToken cancellationToken) =>
        ExecuteAsync(
            new IdRequest { Id = id },
            _userAccessService.GetUserCompaniesAsync,
            cancellationToken);

    [HttpPut("{id:int}/companies")]
    [Authorize(Policy = ImecePolicies.RequireOrganizationManage)]
    public Task<IActionResult> UpdateCompanies(
        int id,
        [FromBody] UpdateUserCompaniesDto request,
        CancellationToken cancellationToken) =>
        ExecuteAsync(
            new UpdateUserCompaniesRequest
            {
                UserId = id,
                CompanyIds = request.CompanyIds
            },
            _userAccessService.UpdateUserCompaniesAsync,
            cancellationToken);
}
