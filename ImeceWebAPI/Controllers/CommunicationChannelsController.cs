using Application.DTOs;
using Application.Services;
using Core.Authorization;
using Core.Common;
using ImeceWebAPI.Controllers.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ImeceWebAPI.Controllers;

[ApiController]
[Route("api/communication-channels/")]
[Authorize(Policy = ImecePolicies.RequireRegisteredUser)]
public sealed class CommunicationChannelsController : ApiControllerBase
{
    private readonly CommunicationChannelService _communicationChannelService;

    public CommunicationChannelsController(CommunicationChannelService communicationChannelService)
    {
        _communicationChannelService = communicationChannelService;
    }

    [HttpGet("get-all-channels")]
    public Task<IActionResult> GetAll(CancellationToken cancellationToken)
        => ExecuteAsync(_communicationChannelService.GetAllAsync, cancellationToken);

    [HttpGet("get-channel-by-id/{id:long}")]
    public Task<IActionResult> GetById(long id, CancellationToken cancellationToken)
        => ExecuteAsync(new IdRequest { Id = id }, _communicationChannelService.GetByIdAsync, cancellationToken);

    [HttpPost("create-channel")]
    [Authorize(Policy = ImecePolicies.RequireCompanyAdmin)]
    public Task<IActionResult> Create([FromBody] CreateCommunicationChannelDto request, CancellationToken cancellationToken)
        => ExecuteAsync(request, _communicationChannelService.CreateAsync, cancellationToken);

    [HttpPut("update-channel-by-id/{id:long}")]
    [Authorize(Policy = ImecePolicies.RequireCompanyAdmin)]
    public Task<IActionResult> Update(long id, [FromBody] UpdateCommunicationChannelDto request, CancellationToken cancellationToken)
    {
        request.ChannelId = id;
        return ExecuteAsync(request, _communicationChannelService.UpdateAsync, cancellationToken);
    }

    [HttpDelete("delete-channel-by-id/{id:long}")]
    [Authorize(Policy = ImecePolicies.RequireCompanyAdmin)]
    public Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
        => ExecuteAsync(new IdRequest { Id = id }, _communicationChannelService.DeleteAsync, cancellationToken);
}
