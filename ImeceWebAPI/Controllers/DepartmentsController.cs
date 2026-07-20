using Application.DTOs;
using Application.Services;
using Core.Authorization;
using ImeceWebAPI.Controllers.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ImeceWebAPI.Controllers;

[ApiController]
[Route("api/departments/")]
[Authorize(Policy = ImecePolicies.RequireRegisteredUser)]
public sealed class DepartmentsController : ApiControllerBase
{
    private readonly DepartmentService _departmentService;

    public DepartmentsController(DepartmentService departmentService)
    {
        _departmentService = departmentService;
    }

    [HttpGet("get-all-departments")]
    public Task<IActionResult> GetAll(CancellationToken cancellationToken) =>
        ExecuteAsync(_departmentService.GetAllAsync, cancellationToken);

    [HttpGet("get-active-departments")]
    public Task<IActionResult> GetActive(CancellationToken cancellationToken) =>
        ExecuteAsync(_departmentService.GetActiveAsync, cancellationToken);

    [HttpGet("get-departments-by-branch/{branchId:int}")]
    public Task<IActionResult> GetByBranch(int branchId, CancellationToken cancellationToken) =>
        ExecuteAsync(
            new BranchIdRequest { BranchId = branchId },
            _departmentService.GetByBranchIdAsync,
            cancellationToken);

    [HttpGet("get-departments-by-company/{companyId:int}")]
    public Task<IActionResult> GetByCompany(int companyId, CancellationToken cancellationToken) =>
        ExecuteAsync(
            new CompanyIdRequest { CompanyId = companyId },
            _departmentService.GetByCompanyIdAsync,
            cancellationToken);

    [HttpGet("get-department-by-id/{id:int}")]
    public Task<IActionResult> GetById(int id, CancellationToken cancellationToken) =>
        ExecuteAsync(new IdRequest { Id = id }, _departmentService.GetByIdAsync, cancellationToken);

    [HttpPost("create-department")]
    [Authorize(Policy = ImecePolicies.RequireCompanyAdmin)]
    public Task<IActionResult> Create(
        [FromBody] CreateDepartmentDto request,
        CancellationToken cancellationToken) =>
        ExecuteAsync(request, _departmentService.CreateAsync, cancellationToken);

    [HttpPut("update-department-by-id/{id:int}")]
    [Authorize(Policy = ImecePolicies.RequireCompanyAdmin)]
    public Task<IActionResult> Update(
        int id,
        [FromBody] UpdateDepartmentDto request,
        CancellationToken cancellationToken)
    {
        request.DepartmentId = id;
        return ExecuteAsync(request, _departmentService.UpdateAsync, cancellationToken);
    }

    [HttpDelete("delete-department-by-id/{id:int}")]
    [Authorize(Policy = ImecePolicies.RequireCompanyAdmin)]
    public Task<IActionResult> Delete(int id, CancellationToken cancellationToken) =>
        ExecuteAsync(new IdRequest { Id = id }, _departmentService.DeleteAsync, cancellationToken);
}
