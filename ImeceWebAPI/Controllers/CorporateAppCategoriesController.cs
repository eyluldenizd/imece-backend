using Application.DTOs;
using Application.Services;
using Core.Authorization;
using ImeceWebAPI.Controllers.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ImeceWebAPI.Controllers;

[ApiController]
[Route("api/corporate-app-categories/")]
[Authorize(Policy = ImecePolicies.RequireRegisteredUser)]
public sealed class CorporateAppCategoriesController : ApiControllerBase
{
    private readonly CorporateAppCategoryService _corporateAppCategoryService;

    public CorporateAppCategoriesController(CorporateAppCategoryService corporateAppCategoryService)
    {
        _corporateAppCategoryService = corporateAppCategoryService;
    }

    [HttpGet("")]
    public Task<IActionResult> GetAll(CancellationToken cancellationToken)
        => ExecuteAsync(_corporateAppCategoryService.GetAllAsync, cancellationToken);

    [HttpGet("{id:int}")]
    public Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        => ExecuteAsync(new IdRequest { Id = id }, _corporateAppCategoryService.GetByIdAsync, cancellationToken);

    [HttpPost("")]
    [Authorize(Policy = ImecePolicies.RequireGlobalContentManager)]
    public Task<IActionResult> Create(
        [FromBody] CreateCorporateAppCategoryDto request,
        CancellationToken cancellationToken)
        => ExecuteAsync(request, _corporateAppCategoryService.CreateAsync, cancellationToken);

    [HttpPut("{id:int}")]
    [Authorize(Policy = ImecePolicies.RequireGlobalContentManager)]
    public Task<IActionResult> Update(
        int id,
        [FromBody] UpdateCorporateAppCategoryDto request,
        CancellationToken cancellationToken)
    {
        request.CorporateAppCategoryId = id;
        return ExecuteAsync(request, _corporateAppCategoryService.UpdateAsync, cancellationToken);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = ImecePolicies.RequireGlobalContentManager)]
    public Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        => ExecuteAsync(new IdRequest { Id = id }, _corporateAppCategoryService.DeleteAsync, cancellationToken);
}
