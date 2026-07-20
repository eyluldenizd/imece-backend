using Application.DTOs;
using Application.Services;
using Core.Authorization;
using ImeceWebAPI.Controllers.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ImeceWebAPI.Controllers;

[ApiController]
[Route("api/users/")]
[Authorize(Policy = ImecePolicies.RequireGlobalAdmin)]
public sealed class UsersController : ApiControllerBase
{
    private readonly UserService _userService;

    public UsersController(
        UserService userService)
    {
        _userService = userService;
    }

    [HttpGet("get-all-users")]
    public Task<IActionResult> GetAll(
        CancellationToken cancellationToken)
    {
        return ExecuteAsync(
            _userService.GetAllAsync,
            cancellationToken);
    }

    [HttpGet("get-active-users")]
    public Task<IActionResult> GetActive(
        CancellationToken cancellationToken)
    {
        return ExecuteAsync(
            _userService.GetActiveAsync,
            cancellationToken);
    }

    [HttpGet("lookup")]
    public Task<IActionResult> Lookup(
        CancellationToken cancellationToken)
    {
        return ExecuteAsync(
            _userService.GetLookupAsync,
            cancellationToken);
    }

    [HttpGet("get-user-by-id/{id:int}")]
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
}
