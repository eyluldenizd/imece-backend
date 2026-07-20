using Application.DTOs;
using Application.Services;
using Core.Authorization;
using ImeceWebAPI.Controllers.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ImeceWebAPI.Controllers;

[ApiController]
[Route("api/weekly-menus")]
[Authorize(Policy = ImecePolicies.RequireRegisteredUser)]
public sealed class WeeklyMenusController : ApiControllerBase
{
    private readonly WeeklyMenuService _weeklyMenuService;
    private readonly WeeklyMenuItemService _weeklyMenuItemService;

    public WeeklyMenusController(
        WeeklyMenuService weeklyMenuService,
        WeeklyMenuItemService weeklyMenuItemService)
    {
        _weeklyMenuService = weeklyMenuService;
        _weeklyMenuItemService = weeklyMenuItemService;
    }

    [HttpGet]
    public Task<IActionResult> GetAll(CancellationToken cancellationToken) =>
        ExecuteAsync(_weeklyMenuService.GetAllAsync, cancellationToken);

    [HttpGet("{id:long}")]
    public Task<IActionResult> GetById(long id, CancellationToken cancellationToken) =>
        ExecuteAsync(new IdRequest { Id = id }, _weeklyMenuService.GetByIdAsync, cancellationToken);

    [HttpPost]
    [Authorize(Policy = ImecePolicies.RequireCompanyAdmin)]
    public Task<IActionResult> Create(
        [FromBody] CreateWeeklyMenuDto request,
        CancellationToken cancellationToken) =>
        ExecuteAsync(request, _weeklyMenuService.CreateAsync, cancellationToken);

    [HttpPut("{id:long}")]
    [Authorize(Policy = ImecePolicies.RequireCompanyAdmin)]
    public Task<IActionResult> Update(
        long id,
        [FromBody] UpdateWeeklyMenuDto request,
        CancellationToken cancellationToken)
    {
        request.MenuId = id;
        return ExecuteAsync(request, _weeklyMenuService.UpdateAsync, cancellationToken);
    }

    [HttpPost("{id:long}/publish")]
    [Authorize(Policy = ImecePolicies.RequireCompanyAdmin)]
    public Task<IActionResult> Publish(long id, CancellationToken cancellationToken) =>
        ExecuteAsync(
            new WeeklyMenuRouteRequest { MenuId = id },
            _weeklyMenuService.PublishAsync,
            cancellationToken);

    [HttpPost("{id:long}/unpublish")]
    [Authorize(Policy = ImecePolicies.RequireCompanyAdmin)]
    public Task<IActionResult> Unpublish(long id, CancellationToken cancellationToken) =>
        ExecuteAsync(
            new WeeklyMenuRouteRequest { MenuId = id },
            _weeklyMenuService.UnpublishAsync,
            cancellationToken);

    [HttpDelete("{id:long}")]
    [Authorize(Policy = ImecePolicies.RequireCompanyAdmin)]
    public Task<IActionResult> Delete(long id, CancellationToken cancellationToken) =>
        ExecuteAsync(new IdRequest { Id = id }, _weeklyMenuService.DeleteAsync, cancellationToken);

    [HttpPost("{menuId:long}/items")]
    [Authorize(Policy = ImecePolicies.RequireCompanyAdmin)]
    public Task<IActionResult> CreateItem(
        long menuId,
        [FromBody] CreateWeeklyMenuItemDto request,
        CancellationToken cancellationToken)
    {
        request.MenuId = menuId;
        return ExecuteAsync(request, _weeklyMenuItemService.CreateAsync, cancellationToken);
    }

    [HttpPut("{menuId:long}/items/{itemId:long}")]
    [Authorize(Policy = ImecePolicies.RequireCompanyAdmin)]
    public Task<IActionResult> UpdateItem(
        long menuId,
        long itemId,
        [FromBody] UpdateWeeklyMenuItemDto request,
        CancellationToken cancellationToken)
    {
        request.MenuId = menuId;
        request.MenuItemId = itemId;
        return ExecuteAsync(request, _weeklyMenuItemService.UpdateAsync, cancellationToken);
    }

    [HttpDelete("{menuId:long}/items/{itemId:long}")]
    [Authorize(Policy = ImecePolicies.RequireCompanyAdmin)]
    public Task<IActionResult> DeleteItem(
        long menuId,
        long itemId,
        CancellationToken cancellationToken) =>
        ExecuteAsync(
            new WeeklyMenuItemRouteRequest { MenuId = menuId, MenuItemId = itemId },
            _weeklyMenuItemService.DeleteAsync,
            cancellationToken);
}
