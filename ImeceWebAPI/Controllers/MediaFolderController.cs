using Application.DTOs;
using Application.Services;
using Core.Authorization;
using ImeceWebAPI.Controllers.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ImeceWebAPI.Controllers;

[ApiController]
[Route("api/media-folders/")]
[Authorize(Policy = ImecePolicies.RequireRegisteredUser)]
public sealed class MediaFoldersController
    : ApiControllerBase
{
    private readonly MediaFolderService _mediaFolderService;

    public MediaFoldersController(
        MediaFolderService mediaFolderService)
    {
        _mediaFolderService = mediaFolderService;
    }

    [HttpGet("get-all-folders")]
    public Task<IActionResult> GetAll(
        CancellationToken cancellationToken)
    {
        return ExecuteAsync(
            _mediaFolderService.GetAllAsync,
            cancellationToken);
    }

    [HttpGet("get-active-folders")]
    public Task<IActionResult> GetActive(
        CancellationToken cancellationToken)
    {
        return ExecuteAsync(
            _mediaFolderService.GetActiveAsync,
            cancellationToken);
    }

    [HttpGet("get-folder-by-id/{id:long}")]
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
            _mediaFolderService.GetByIdAsync,
            cancellationToken);
    }

    [HttpGet("get-folders-by-company/{companyId:int}")]
    public Task<IActionResult> GetByCompany(
        int companyId,
        CancellationToken cancellationToken)
    {
        var request = new MediaFolderCompanyRequest
        {
            CompanyId = companyId
        };

        return ExecuteAsync(
            request,
            _mediaFolderService.GetByCompanyAsync,
            cancellationToken);
    }

    [HttpGet("get-child-folders/{parentFolderId:long}")]
    public Task<IActionResult> GetChildren(
        long parentFolderId,
        CancellationToken cancellationToken)
    {
        var request = new MediaFolderChildrenRequest
        {
            ParentFolderId = parentFolderId
        };

        return ExecuteAsync(
            request,
            _mediaFolderService.GetChildrenAsync,
            cancellationToken);
    }

    [HttpPost("create-folder")]
    [Authorize(Policy = ImecePolicies.RequireCompanyAdmin)]
    public Task<IActionResult> Create(
        [FromBody] CreateMediaFolderDto request,
        CancellationToken cancellationToken)
    {
        return ExecuteAsync(
            request,
            _mediaFolderService.CreateAsync,
            cancellationToken);
    }

    [HttpPut("update-folder-by-id/{id:long}")]
    [Authorize(Policy = ImecePolicies.RequireCompanyAdmin)]
    public Task<IActionResult> Update(
        long id,
        [FromBody] UpdateMediaFolderDto request,
        CancellationToken cancellationToken)
    {
        request.FolderId = id;

        return ExecuteAsync(
            request,
            _mediaFolderService.UpdateAsync,
            cancellationToken);
    }

    [HttpDelete("delete-folder-by-id/{id:long}")]
    [Authorize(Policy = ImecePolicies.RequireCompanyAdmin)]
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
            _mediaFolderService.DeleteAsync,
            cancellationToken);
    }
}