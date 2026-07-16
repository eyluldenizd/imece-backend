using Application.DTOs;
using Application.Services;
using ImeceWebAPI.Controllers.Common;
using Microsoft.AspNetCore.Mvc;

namespace ImeceWebAPI.Controllers;

[ApiController]
[Route("api/weekly-menu-entries/")]
public sealed class WeeklyMenuEntriesController
    : ApiControllerBase
{
    private readonly WeeklyMenuEntryService
        _weeklyMenuEntryService;

    public WeeklyMenuEntriesController(
        WeeklyMenuEntryService weeklyMenuEntryService)
    {
        _weeklyMenuEntryService =
            weeklyMenuEntryService;
    }

    [HttpGet("get-all-menu-entries")]
    public Task<IActionResult> GetAll(
        CancellationToken cancellationToken)
    {
        return ExecuteAsync(
            _weeklyMenuEntryService.GetAllAsync,
            cancellationToken);
    }

    [HttpGet("get-menu-entry-by-id/{id:long}")]
    public Task<IActionResult> GetById(
        long id,
        CancellationToken cancellationToken)
    {
        var request = new IdRequest
        {
            Id = id
        };

        return ExecuteAsync(
            request,
            _weeklyMenuEntryService.GetByIdAsync,
            cancellationToken);
    }

    [HttpGet("get-current-week-menu")]
    public Task<IActionResult> GetCurrentWeek(
        CancellationToken cancellationToken)
    {
        return ExecuteAsync(
            _weeklyMenuEntryService.GetCurrentWeekAsync,
            cancellationToken);
    }

    [HttpGet("get-menu-by-date/{menuDate}")]
    public Task<IActionResult> GetByDate(
        DateOnly menuDate,
        CancellationToken cancellationToken)
    {
        var request = new WeeklyMenuDateRequest
        {
            MenuDate = menuDate
        };

        return ExecuteAsync(
            request,
            _weeklyMenuEntryService.GetByDateAsync,
            cancellationToken);
    }

    [HttpGet("get-menu-by-branch/{branchId:int}")]
    public Task<IActionResult> GetByBranch(
        int branchId,
        CancellationToken cancellationToken)
    {
        var request = new WeeklyMenuBranchRequest
        {
            BranchId = branchId
        };

        return ExecuteAsync(
            request,
            _weeklyMenuEntryService.GetByBranchAsync,
            cancellationToken);
    }

    [HttpPost("create-menu-entry")]
    public Task<IActionResult> Create(
        [FromBody] CreateWeeklyMenuEntryDto request,
        CancellationToken cancellationToken)
    {
        return ExecuteAsync(
            request,
            _weeklyMenuEntryService.CreateAsync,
            cancellationToken);
    }

    [HttpPut("update-menu-entry-by-id/{id:long}")]
    public Task<IActionResult> Update(
        long id,
        [FromBody] UpdateWeeklyMenuEntryDto request,
        CancellationToken cancellationToken)
    {
        request.EntryId = id;

        return ExecuteAsync(
            request,
            _weeklyMenuEntryService.UpdateAsync,
            cancellationToken);
    }

    [HttpDelete("delete-menu-entry-by-id/{id:long}")]
    public Task<IActionResult> Delete(
        long id,
        CancellationToken cancellationToken)
    {
        var request = new IdRequest
        {
            Id = id
        };

        return ExecuteAsync(
            request,
            _weeklyMenuEntryService.DeleteAsync,
            cancellationToken);
    }
}