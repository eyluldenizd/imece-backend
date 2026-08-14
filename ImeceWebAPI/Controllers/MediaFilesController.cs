using Application.DTOs;
using Application.Services;
using Core.Authorization;
using Core.Common;
using ImeceWebAPI.Controllers.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ImeceWebAPI.Controllers;

[ApiController]
[Route("api/media-files/")]
[Authorize(Policy = ImecePolicies.RequireRegisteredUser)]
public sealed class MediaFilesController
    : ApiControllerBase
{
    private readonly MediaFileService _mediaFileService;

    public MediaFilesController(
        MediaFileService mediaFileService)
    {
        _mediaFileService = mediaFileService;
    }

    [HttpGet("get-all-files")]
    [Authorize(Policy = ImecePolicies.RequireMediaView)]
    public Task<IActionResult> GetAll(
        [FromQuery] ContentListQueryDto query,
        CancellationToken cancellationToken)
    {
        return ExecuteAsync(
            token => _mediaFileService.GetAllAsync(query, token),
            cancellationToken);
    }

    [HttpGet("get-active-files")]
    [Authorize(Policy = ImecePolicies.RequireMediaView)]
    public Task<IActionResult> GetActive(
        CancellationToken cancellationToken)
    {
        return ExecuteAsync(
            _mediaFileService.GetActiveAsync,
            cancellationToken);
    }

    [HttpGet("get-file-by-id/{id:long}")]
    [Authorize(Policy = ImecePolicies.RequireMediaView)]
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
            _mediaFileService.GetByIdAsync,
            cancellationToken);
    }

    [HttpGet("get-files-by-company/{companyId:int}")]
    [Authorize(Policy = ImecePolicies.RequireMediaView)]
    public Task<IActionResult> GetByCompany(
        int companyId,
        CancellationToken cancellationToken)
    {
        var request = new MediaFileCompanyRequest
        {
            CompanyId = companyId
        };

        return ExecuteAsync(
            request,
            _mediaFileService.GetByCompanyAsync,
            cancellationToken);
    }

    [HttpGet("get-files-by-folder/{folderId:long}")]
    [Authorize(Policy = ImecePolicies.RequireMediaView)]
    public Task<IActionResult> GetByFolder(
        long folderId,
        CancellationToken cancellationToken)
    {
        var request = new MediaFileFolderRequest
        {
            FolderId = folderId
        };

        return ExecuteAsync(
            request,
            _mediaFileService.GetByFolderAsync,
            cancellationToken);
    }

    [HttpGet("get-files-by-type/{mediaType}")]
    [Authorize(Policy = ImecePolicies.RequireMediaView)]
    public Task<IActionResult> GetByMediaType(
        string mediaType,
        CancellationToken cancellationToken)
    {
        var request = new MediaFileTypeRequest
        {
            MediaType = mediaType
        };

        return ExecuteAsync(
            request,
            _mediaFileService.GetByMediaTypeAsync,
            cancellationToken);
    }

    [HttpGet("search-files")]
    [Authorize(Policy = ImecePolicies.RequireMediaView)]
    public Task<IActionResult> Search(
        [FromQuery] string searchText,
        CancellationToken cancellationToken)
    {
        var request = new MediaFileSearchRequest
        {
            SearchText = searchText
        };

        return ExecuteAsync(
            request,
            _mediaFileService.SearchAsync,
            cancellationToken);
    }

    [HttpPost("upload")]
    [Authorize(Policy = ImecePolicies.RequireMediaManage)]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(26_214_400)]
    public Task<IActionResult> Upload(
        [FromForm] UploadMediaFileDto request,
        CancellationToken cancellationToken)
    {
        return ExecuteAsync(
            request,
            _mediaFileService.UploadAsync,
            cancellationToken);
    }

    [HttpPut("update-file-by-id/{id:long}")]
    [Authorize(Policy = ImecePolicies.RequireMediaManage)]
    public Task<IActionResult> Update(
        long id,
        [FromBody] UpdateMediaFileDto request,
        CancellationToken cancellationToken)
    {
        request.MediaFileId = id;

        return ExecuteAsync(
            request,
            _mediaFileService.UpdateAsync,
            cancellationToken);
    }

    [HttpDelete("delete-file-by-id/{id:long}")]
    [Authorize(Policy = ImecePolicies.RequireMediaManage)]
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
            _mediaFileService.DeleteAsync,
            cancellationToken);
    }

    [HttpGet("download/{id:long}")]
    [Authorize(Policy = ImecePolicies.RequireMediaView)]
    public async Task<IActionResult> Download(long id, CancellationToken cancellationToken)
    {
        var result = await _mediaFileService.OpenDownloadAsync(
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
