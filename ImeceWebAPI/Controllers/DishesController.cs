using Application.DTOs;
using Application.Services;
using Core.Authorization;
using ImeceWebAPI.Controllers.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ImeceWebAPI.Controllers;

[ApiController]
[Route("api/dishes")]
[Authorize(Policy = ImecePolicies.RequireRegisteredUser)]
public sealed class DishesController : ApiControllerBase
{
    private readonly DishesService _dishesService;

    public DishesController(DishesService dishesService)
    {
        _dishesService = dishesService;
    }

    [HttpGet]
    public Task<IActionResult> GetAll(CancellationToken cancellationToken) =>
        ExecuteAsync(_dishesService.GetAllAsync, cancellationToken);

    [HttpGet("active")]
    public Task<IActionResult> GetActive(CancellationToken cancellationToken) =>
        ExecuteAsync(_dishesService.GetActiveAsync, cancellationToken);

    [HttpGet("{id:int}")]
    public Task<IActionResult> GetById(int id, CancellationToken cancellationToken) =>
        ExecuteAsync(new IdRequest { Id = id }, _dishesService.GetByIdAsync, cancellationToken);

    [HttpPost]
    [Authorize(Policy = ImecePolicies.RequireGlobalContentManager)]
    public Task<IActionResult> Create(
        [FromBody] CreateDishesDto request,
        CancellationToken cancellationToken) =>
        ExecuteAsync(request, _dishesService.CreateAsync, cancellationToken);

    [HttpPut("{id:int}")]
    [Authorize(Policy = ImecePolicies.RequireGlobalContentManager)]
    public Task<IActionResult> Update(
        int id,
        [FromBody] UpdateDishesDto request,
        CancellationToken cancellationToken)
    {
        request.DishId = id;
        return ExecuteAsync(request, _dishesService.UpdateAsync, cancellationToken);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = ImecePolicies.RequireGlobalContentManager)]
    public Task<IActionResult> Delete(int id, CancellationToken cancellationToken) =>
        ExecuteAsync(new IdRequest { Id = id }, _dishesService.DeleteAsync, cancellationToken);
}
