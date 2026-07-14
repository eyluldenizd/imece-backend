using Application.DTOs;
using Application.Services;
using ImeceWebAPI.Controllers.Common;
using Microsoft.AspNetCore.Mvc;

namespace ImeceWebAPI.Controllers;

[ApiController]
[Route("api/media-files/")]
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
    public Task<IActionResult> GetAll(
        CancellationToken cancellationToken)
    {
        return ExecuteAsync(
            _mediaFileService.GetAllAsync,
            cancellationToken);
    }

    [HttpGet("get-active-files")]
    public Task<IActionResult> GetActive(
        CancellationToken cancellationToken)
    {
        return ExecuteAsync(
            _mediaFileService.GetActiveAsync,
            cancellationToken);
    }

    [HttpGet("get-file-by-id/{id:long}")]
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

    [HttpPut("update-file-by-id/{id:long}")]
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
}