using Application.DTOs;
using Application.Services;
using ImeceWebAPI.Controllers.Common;
using Microsoft.AspNetCore.Mvc;

namespace ImeceWebAPI.Controllers;

[ApiController]
[Route("api/users/")]
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