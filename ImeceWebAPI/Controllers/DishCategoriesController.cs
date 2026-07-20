using Application.DTOs;
using Application.Services;
using Core.Authorization;
using ImeceWebAPI.Controllers.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ImeceWebAPI.Controllers;

[ApiController]
[Route("api/dish-categories")]
[Authorize(Policy = ImecePolicies.RequireRegisteredUser)]
public sealed class DishCategoriesController : ApiControllerBase
{
    private readonly DishCategoryService _dishCategoryService;

    public DishCategoriesController(DishCategoryService dishCategoryService)
    {
        _dishCategoryService = dishCategoryService;
    }

    [HttpGet]
    public Task<IActionResult> GetAll(CancellationToken cancellationToken) =>
        ExecuteAsync(_dishCategoryService.GetAllAsync, cancellationToken);

    [HttpGet("{id:int}")]
    public Task<IActionResult> GetById(int id, CancellationToken cancellationToken) =>
        ExecuteAsync(new IdRequest { Id = id }, _dishCategoryService.GetByIdAsync, cancellationToken);

    [HttpPost]
    [Authorize(Policy = ImecePolicies.RequireGlobalContentManager)]
    public Task<IActionResult> Create(
        [FromBody] CreateDishCategoryDto request,
        CancellationToken cancellationToken) =>
        ExecuteAsync(request, _dishCategoryService.CreateAsync, cancellationToken);

    [HttpPut("{id:int}")]
    [Authorize(Policy = ImecePolicies.RequireGlobalContentManager)]
    public Task<IActionResult> Update(
        int id,
        [FromBody] UpdateDishCategoryDto request,
        CancellationToken cancellationToken)
    {
        request.DishCategoryId = id;
        return ExecuteAsync(request, _dishCategoryService.UpdateAsync, cancellationToken);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = ImecePolicies.RequireGlobalContentManager)]
    public Task<IActionResult> Delete(int id, CancellationToken cancellationToken) =>
        ExecuteAsync(new IdRequest { Id = id }, _dishCategoryService.DeleteAsync, cancellationToken);
}
