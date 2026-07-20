using Application.DTOs;
using Application.Services;
using Core.Authorization;
using ImeceWebAPI.Controllers.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ImeceWebAPI.Controllers;

[ApiController]
[Route("api/branches/")]
[Authorize(Policy = ImecePolicies.RequireRegisteredUser)]
public sealed class BranchesController : ApiControllerBase
{
    private readonly BranchService _branchService;

    public BranchesController(BranchService branchService)
    {
        _branchService = branchService;
    }

    [HttpGet("get-all-branches")]
    public Task<IActionResult> GetAll(CancellationToken cancellationToken) =>
        ExecuteAsync(_branchService.GetAllAsync, cancellationToken);

    [HttpGet("get-active-branches")]
    public Task<IActionResult> GetActive(CancellationToken cancellationToken) =>
        ExecuteAsync(_branchService.GetActiveAsync, cancellationToken);

    [HttpGet("get-branches-by-company/{companyId:int}")]
    public Task<IActionResult> GetByCompany(int companyId, CancellationToken cancellationToken) =>
        ExecuteAsync(
            new CompanyIdRequest { CompanyId = companyId },
            _branchService.GetByCompanyIdAsync,
            cancellationToken);

    [HttpGet("get-branch-by-id/{id:int}")]
    public Task<IActionResult> GetById(int id, CancellationToken cancellationToken) =>
        ExecuteAsync(new IdRequest { Id = id }, _branchService.GetByIdAsync, cancellationToken);

    [HttpPost("create-branch")]
    [Authorize(Policy = ImecePolicies.RequireCompanyAdmin)]
    public Task<IActionResult> Create(
        [FromBody] CreateBranchDto request,
        CancellationToken cancellationToken) =>
        ExecuteAsync(request, _branchService.CreateAsync, cancellationToken);

    [HttpPut("update-branch-by-id/{id:int}")]
    [Authorize(Policy = ImecePolicies.RequireCompanyAdmin)]
    public Task<IActionResult> Update(
        int id,
        [FromBody] UpdateBranchDto request,
        CancellationToken cancellationToken)
    {
        request.BranchId = id;
        return ExecuteAsync(request, _branchService.UpdateAsync, cancellationToken);
    }

    [HttpDelete("delete-branch-by-id/{id:int}")]
    [Authorize(Policy = ImecePolicies.RequireCompanyAdmin)]
    public Task<IActionResult> Delete(int id, CancellationToken cancellationToken) =>
        ExecuteAsync(new IdRequest { Id = id }, _branchService.DeleteAsync, cancellationToken);
}
