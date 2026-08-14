using Application.DTOs;
using Application.Services;
using Core.Authorization;
using Core.Common;
using ImeceWebAPI.Controllers.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ImeceWebAPI.Controllers;

[ApiController]
[Route("api/library/")]
[Authorize(Policy = ImecePolicies.RequireRegisteredUser)]
public sealed class LibraryController : ApiControllerBase
{
    private readonly LibraryService _libraryService;

    public LibraryController(LibraryService libraryService)
    {
        _libraryService = libraryService;
    }

    [HttpGet("")]
    [Authorize(Policy = ImecePolicies.RequireMediaView)]
    public Task<IActionResult> GetAll(
        [FromQuery] ContentListQueryDto query,
        CancellationToken cancellationToken)
        => ExecuteAsync(
            token => _libraryService.GetAllAsync(query, token),
            cancellationToken);

    [HttpGet("{id:long}")]
    [Authorize(Policy = ImecePolicies.RequireMediaView)]
    public Task<IActionResult> GetById(long id, CancellationToken cancellationToken)
        => ExecuteAsync(
            new IdRequest { Id = id },
            _libraryService.GetByIdAsync,
            cancellationToken);

    [HttpPost("upload")]
    [Authorize(Policy = ImecePolicies.RequireMediaManage)]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(26_214_400)]
    public Task<IActionResult> Upload(
        [FromForm] UploadMediaFileDto request,
        CancellationToken cancellationToken)
        => ExecuteAsync(request, _libraryService.UploadAsync, cancellationToken);

    [HttpPut("{id:long}")]
    [Authorize(Policy = ImecePolicies.RequireMediaManage)]
    public Task<IActionResult> Update(
        long id,
        [FromBody] UpdateMediaFileDto request,
        CancellationToken cancellationToken)
    {
        request.MediaFileId = id;
        return ExecuteAsync(request, _libraryService.UpdateAsync, cancellationToken);
    }

    [HttpDelete("{id:long}")]
    [Authorize(Policy = ImecePolicies.RequireMediaManage)]
    public Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
        => ExecuteAsync(
            new IdRequest { Id = id },
            _libraryService.DeleteAsync,
            cancellationToken);

    [HttpGet("{id:long}/download")]
    [Authorize(Policy = ImecePolicies.RequireMediaView)]
    public async Task<IActionResult> Download(long id, CancellationToken cancellationToken)
    {
        var result = await _libraryService.OpenDownloadAsync(
            new IdRequest { Id = id },
            cancellationToken);

        if (result.StatusCode != StatusCodeEnum.Success || result.Data is null)
        {
            return result.StatusCode switch
            {
                StatusCodeEnum.NotFound => NotFound(new { message = result.Message }),
                _ => BadRequest(new { message = result.Message }),
            };
        }

        Response.RegisterForDispose(result.Data);
        return File(result.Data.Content, result.Data.ContentType, result.Data.FileName);
    }
}
