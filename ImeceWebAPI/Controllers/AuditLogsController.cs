using Application.DTOs;
using Application.Services;
using Core.Authorization;
using ImeceWebAPI.Controllers.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ImeceWebAPI.Controllers;

[ApiController]
[Route("api/audit-logs/")]
[Authorize(Policy = ImecePolicies.RequireRegisteredUser)]
public sealed class AuditLogsController : ApiControllerBase
{
    private readonly AuditLogService _auditLogService;

    public AuditLogsController(AuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    [HttpGet("")]
    [Authorize(Policy = ImecePolicies.RequireAdminPanelAccess)]
    public Task<IActionResult> GetPaged(
        [FromQuery] AuditLogListQueryDto query,
        CancellationToken cancellationToken)
        => ExecuteAsync(
            token => _auditLogService.GetPagedAsync(query, token),
            cancellationToken);

    [HttpGet("filter-options")]
    [Authorize(Policy = ImecePolicies.RequireAdminPanelAccess)]
    public Task<IActionResult> GetFilterOptions(CancellationToken cancellationToken)
        => ExecuteAsync(
            token => _auditLogService.GetFilterOptionsAsync(token),
            cancellationToken);

    [HttpGet("{id:long}")]
    [Authorize(Policy = ImecePolicies.RequireAdminPanelAccess)]
    public Task<IActionResult> GetById(long id, CancellationToken cancellationToken)
        => ExecuteAsync(
            new IdRequest { Id = id },
            _auditLogService.GetByIdAsync,
            cancellationToken);
}
