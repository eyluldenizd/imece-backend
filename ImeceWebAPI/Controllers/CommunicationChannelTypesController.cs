using Application.DTOs;
using Application.Services;
using Core.Authorization;
using ImeceWebAPI.Controllers.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ImeceWebAPI.Controllers;

[ApiController]
[Route("api/communication-channel-types/")]
[Authorize(Policy = ImecePolicies.RequireRegisteredUser)]
public sealed class CommunicationChannelTypesController : ApiControllerBase
{
    private readonly CommunicationChannelTypeService _communicationChannelTypeService;

    public CommunicationChannelTypesController(CommunicationChannelTypeService communicationChannelTypeService)
    {
        _communicationChannelTypeService = communicationChannelTypeService;
    }

    [HttpGet("")]
    public Task<IActionResult> GetAll(CancellationToken cancellationToken)
        => ExecuteAsync(_communicationChannelTypeService.GetAllAsync, cancellationToken);

    [HttpGet("{id:int}")]
    public Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        => ExecuteAsync(new IdRequest { Id = id }, _communicationChannelTypeService.GetByIdAsync, cancellationToken);

    [HttpPost("")]
    [Authorize(Policy = ImecePolicies.RequireGlobalContentManager)]
    public Task<IActionResult> Create(
        [FromBody] CreateCommunicationChannelTypeDto request,
        CancellationToken cancellationToken)
        => ExecuteAsync(request, _communicationChannelTypeService.CreateAsync, cancellationToken);

    [HttpPut("{id:int}")]
    [Authorize(Policy = ImecePolicies.RequireGlobalContentManager)]
    public Task<IActionResult> Update(
        int id,
        [FromBody] UpdateCommunicationChannelTypeDto request,
        CancellationToken cancellationToken)
    {
        request.CommunicationChannelTypeId = id;
        return ExecuteAsync(request, _communicationChannelTypeService.UpdateAsync, cancellationToken);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = ImecePolicies.RequireGlobalContentManager)]
    public Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        => ExecuteAsync(new IdRequest { Id = id }, _communicationChannelTypeService.DeleteAsync, cancellationToken);
}
