using Application.Services;
using Core.Authorization;
using ImeceWebAPI.Controllers.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ImeceWebAPI.Controllers;

[ApiController]
[Route("api/dashboard/")]
[Authorize(Policy = ImecePolicies.RequireRegisteredUser)]
public sealed class DashboardController : ApiControllerBase
{
    private readonly DashboardService _dashboardService;

    public DashboardController(DashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("summary")]
    public Task<IActionResult> GetSummary(CancellationToken cancellationToken)
        => ExecuteAsync(_dashboardService.GetSummaryAsync, cancellationToken);
}
