using Application.DTOs;
using Application.Services;
using Core.Authorization;
using ImeceWebAPI.Controllers.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ImeceWebAPI.Controllers;

[ApiController]
[Route("api/emergency-number-categories/")]
[Authorize(Policy = ImecePolicies.RequireRegisteredUser)]
public sealed class EmergencyNumberCategoriesController : ApiControllerBase
{
    private readonly EmergencyNumberCategoryService _emergencyNumberCategoryService;

    public EmergencyNumberCategoriesController(EmergencyNumberCategoryService emergencyNumberCategoryService)
    {
        _emergencyNumberCategoryService = emergencyNumberCategoryService;
    }

    [HttpGet("")]
    [Authorize(Policy = ImecePolicies.RequireContentView)]
    public Task<IActionResult> GetAll(
        [FromQuery] ContentListQueryDto query,
        CancellationToken cancellationToken)
        => ExecuteAsync(
            token => _emergencyNumberCategoryService.GetAllAsync(query, token),
            cancellationToken);

    [HttpGet("{id:int}")]
    [Authorize(Policy = ImecePolicies.RequireContentView)]
    public Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        => ExecuteAsync(new IdRequest { Id = id }, _emergencyNumberCategoryService.GetByIdAsync, cancellationToken);

    [HttpPost("")]
    [Authorize(Policy = ImecePolicies.RequireContentGlobalManage)]
    public Task<IActionResult> Create(
        [FromBody] CreateEmergencyNumberCategoryDto request,
        CancellationToken cancellationToken)
        => ExecuteAsync(request, _emergencyNumberCategoryService.CreateAsync, cancellationToken);

    [HttpPut("{id:int}")]
    [Authorize(Policy = ImecePolicies.RequireContentGlobalManage)]
    public Task<IActionResult> Update(
        int id,
        [FromBody] UpdateEmergencyNumberCategoryDto request,
        CancellationToken cancellationToken)
    {
        request.EmergencyNumberCategoryId = id;
        return ExecuteAsync(request, _emergencyNumberCategoryService.UpdateAsync, cancellationToken);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = ImecePolicies.RequireContentGlobalManage)]
    public Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        => ExecuteAsync(new IdRequest { Id = id }, _emergencyNumberCategoryService.DeleteAsync, cancellationToken);
}
