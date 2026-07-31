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
    [Authorize(Policy = ImecePolicies.RequireOrganizationView)]
    public Task<IActionResult> GetAll(
        [FromQuery] ContentListQueryDto query,
        CancellationToken cancellationToken) =>
        ExecuteAsync(
            token => _departmentService.GetAllAsync(query, token),
            cancellationToken);

    [HttpGet("get-active-departments")]
    [Authorize(Policy = ImecePolicies.RequireOrganizationView)]
    public Task<IActionResult> GetActive(CancellationToken cancellationToken) =>
        ExecuteAsync(_departmentService.GetActiveAsync, cancellationToken);

    [HttpGet("get-departments-by-branch/{branchId:int}")]
    [Authorize(Policy = ImecePolicies.RequireOrganizationView)]
    public Task<IActionResult> GetByBranch(int branchId, CancellationToken cancellationToken) =>
        ExecuteAsync(
            new BranchIdRequest { BranchId = branchId },
            _departmentService.GetByBranchIdAsync,
            cancellationToken);

    [HttpGet("get-departments-by-company/{companyId:int}")]
    [Authorize(Policy = ImecePolicies.RequireOrganizationView)]
    public Task<IActionResult> GetByCompany(int companyId, CancellationToken cancellationToken) =>
        ExecuteAsync(
            new CompanyIdRequest { CompanyId = companyId },
            _departmentService.GetByCompanyIdAsync,
            cancellationToken);

    [HttpGet("get-department-by-id/{id:int}")]
    [Authorize(Policy = ImecePolicies.RequireOrganizationView)]
    public Task<IActionResult> GetById(int id, CancellationToken cancellationToken) =>
        ExecuteAsync(new IdRequest { Id = id }, _departmentService.GetByIdAsync, cancellationToken);

    [HttpPost("create-department")]
    [Authorize(Policy = ImecePolicies.RequireOrganizationManage)]
    public Task<IActionResult> Create(
        [FromBody] CreateDepartmentDto request,
        CancellationToken cancellationToken) =>
        ExecuteAsync(request, _departmentService.CreateAsync, cancellationToken);

    [HttpPut("update-department-by-id/{id:int}")]
    [Authorize(Policy = ImecePolicies.RequireOrganizationManage)]
    public Task<IActionResult> Update(
        int id,
        [FromBody] UpdateDepartmentDto request,
        CancellationToken cancellationToken)
    {
        request.DepartmentId = id;
        return ExecuteAsync(request, _departmentService.UpdateAsync, cancellationToken);
    }

    [HttpDelete("delete-department-by-id/{id:int}")]
    [Authorize(Policy = ImecePolicies.RequireOrganizationManage)]
    public Task<IActionResult> Delete(int id, CancellationToken cancellationToken) =>
        ExecuteAsync(new IdRequest { Id = id }, _departmentService.DeleteAsync, cancellationToken);
}
