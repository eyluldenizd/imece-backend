using Application.Services;
using ImeceWebAPI.Controllers.Common;
using Microsoft.AspNetCore.Mvc;

namespace ImeceWebAPI.Controllers;

[ApiController]
[Route("api/communication-channels/")]
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
}
