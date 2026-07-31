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
    [Authorize(Policy = ImecePolicies.RequireOrganizationView)]
    public Task<IActionResult> GetAll(
        [FromQuery] ContentListQueryDto query,
        CancellationToken cancellationToken) =>
        ExecuteAsync(
            token => _branchService.GetAllAsync(query, token),
            cancellationToken);

    [HttpGet("get-active-branches")]
    [Authorize(Policy = ImecePolicies.RequireOrganizationView)]
    public Task<IActionResult> GetActive(CancellationToken cancellationToken) =>
        ExecuteAsync(_branchService.GetActiveAsync, cancellationToken);

    [HttpGet("get-branches-by-company/{companyId:int}")]
    [Authorize(Policy = ImecePolicies.RequireOrganizationView)]
    public Task<IActionResult> GetByCompany(int companyId, CancellationToken cancellationToken) =>
        ExecuteAsync(
            new CompanyIdRequest { CompanyId = companyId },
            _branchService.GetByCompanyIdAsync,
            cancellationToken);

    [HttpGet("get-branch-by-id/{id:int}")]
    [Authorize(Policy = ImecePolicies.RequireOrganizationView)]
    public Task<IActionResult> GetById(int id, CancellationToken cancellationToken) =>
        ExecuteAsync(new IdRequest { Id = id }, _branchService.GetByIdAsync, cancellationToken);

    [HttpPost("create-branch")]
    [Authorize(Policy = ImecePolicies.RequireOrganizationManage)]
    public Task<IActionResult> Create(
        [FromBody] CreateBranchDto request,
        CancellationToken cancellationToken) =>
        ExecuteAsync(request, _branchService.CreateAsync, cancellationToken);

    [HttpPut("update-branch-by-id/{id:int}")]
    [Authorize(Policy = ImecePolicies.RequireOrganizationManage)]
    public Task<IActionResult> Update(
        int id,
        [FromBody] UpdateBranchDto request,
        CancellationToken cancellationToken)
    {
        request.BranchId = id;
        return ExecuteAsync(request, _branchService.UpdateAsync, cancellationToken);
    }

    [HttpDelete("delete-branch-by-id/{id:int}")]
    [Authorize(Policy = ImecePolicies.RequireOrganizationManage)]
    public Task<IActionResult> Delete(int id, CancellationToken cancellationToken) =>
        ExecuteAsync(new IdRequest { Id = id }, _branchService.DeleteAsync, cancellationToken);
}
