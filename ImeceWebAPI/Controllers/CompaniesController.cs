using Application.DTOs;
using Application.Services;
using Core.Authorization;
using ImeceWebAPI.Controllers.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ImeceWebAPI.Controllers;

[ApiController]
[Route("api/companies/")]
[Authorize(Policy = ImecePolicies.RequireRegisteredUser)]
public sealed class CompaniesController : ApiControllerBase
{
    private readonly CompanyService _companyService;

    public CompaniesController(CompanyService companyService)
    {
        _companyService = companyService;
    }

    [HttpGet("get-all-companies")]
    public Task<IActionResult> GetAll(CancellationToken cancellationToken) =>
        ExecuteAsync(_companyService.GetAllAsync, cancellationToken);

    [HttpGet("get-active-companies")]
    public Task<IActionResult> GetActive(CancellationToken cancellationToken) =>
        ExecuteAsync(_companyService.GetActiveAsync, cancellationToken);

    [HttpGet("get-company-by-id/{id:int}")]
    public Task<IActionResult> GetById(int id, CancellationToken cancellationToken) =>
        ExecuteAsync(new IdRequest { Id = id }, _companyService.GetByIdAsync, cancellationToken);

    [HttpPost("create-company")]
    [Authorize(Policy = ImecePolicies.RequireGlobalAdmin)]
    public Task<IActionResult> Create(
        [FromBody] CreateCompanyDto request,
        CancellationToken cancellationToken) =>
        ExecuteAsync(request, _companyService.CreateAsync, cancellationToken);

    [HttpPut("update-company-by-id/{id:int}")]
    [Authorize(Policy = ImecePolicies.RequireGlobalAdmin)]
    public Task<IActionResult> Update(
        int id,
        [FromBody] UpdateCompanyDto request,
        CancellationToken cancellationToken)
    {
        request.CompanyId = id;
        return ExecuteAsync(request, _companyService.UpdateAsync, cancellationToken);
    }

    [HttpDelete("delete-company-by-id/{id:int}")]
    [Authorize(Policy = ImecePolicies.RequireGlobalAdmin)]
    public Task<IActionResult> Delete(int id, CancellationToken cancellationToken) =>
        ExecuteAsync(new IdRequest { Id = id }, _companyService.DeleteAsync, cancellationToken);
}
