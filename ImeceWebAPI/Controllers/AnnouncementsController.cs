using Application.DTOs;
using Application.Services;
using Core.Authorization;
using ImeceWebAPI.Controllers.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ImeceWebAPI.Controllers;

[ApiController]
[Route("api/announcements")]
[Authorize(Policy = ImecePolicies.RequireRegisteredUser)]
public sealed class AnnouncementsController : ApiControllerBase
{
    private readonly AnnouncementService _announcementService;

    public AnnouncementsController(
        AnnouncementService announcementService)
    {
        _announcementService = announcementService;
    }

    [HttpGet]
    [Authorize(Policy = ImecePolicies.RequireContentView)]
    public Task<IActionResult> GetAll(
        [FromQuery] ContentListQueryDto query,
        CancellationToken cancellationToken)
    {
        return ExecuteAsync(
            token => _announcementService.GetAllAsync(query, token),
            cancellationToken);
    }

    [HttpGet("published")]
    [Authorize(Policy = ImecePolicies.RequireContentView)]
    public Task<IActionResult> GetPublished(
        CancellationToken cancellationToken)
    {
        return ExecuteAsync(
            _announcementService.GetPublishedAsync,
            cancellationToken);
    }

    [HttpGet("{id:long}")]
    [Authorize(Policy = ImecePolicies.RequireContentView)]
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
            _announcementService.GetByIdAsync,
            cancellationToken);
    }

    [HttpPost]
    [Authorize(Policy = ImecePolicies.RequireContentCompanyManage)]
    public Task<IActionResult> Create(
        [FromBody] CreateAnnouncementDto request,
        CancellationToken cancellationToken)
    {
        return ExecuteAsync(
            request,
            _announcementService.CreateAsync,
            cancellationToken);
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = ImecePolicies.RequireContentCompanyManage)]
    public Task<IActionResult> Update(
        long id,
        [FromBody] UpdateAnnouncementDto request,
        CancellationToken cancellationToken)
    {
        request.AnnouncementId = id;

        return ExecuteAsync(
            request,
            _announcementService.UpdateAsync,
            cancellationToken);
    }

    [HttpDelete("{id:long}")]
    [Authorize(Policy = ImecePolicies.RequireContentCompanyManage)]
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
            _announcementService.DeleteAsync,
            cancellationToken);
    }
}