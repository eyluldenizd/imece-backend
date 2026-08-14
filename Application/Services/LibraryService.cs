using Application.DTOs;
using Core.Common;

namespace Application.Services;

/// <summary>
/// Dokümanlar (library) — media_files where MediaType=Document.
/// </summary>
public sealed class LibraryService
{
    private readonly MediaFileService _mediaFileService;

    public LibraryService(MediaFileService mediaFileService)
    {
        _mediaFileService = mediaFileService;
    }

    public Task<ServiceResult<IReadOnlyList<MediaFileDto>>> GetAllAsync(
        ContentListQueryDto? query = null,
        CancellationToken cancellationToken = default)
    {
        query ??= new ContentListQueryDto();
        query.FeatureType = "Document";
        return _mediaFileService.GetAllAsync(query, cancellationToken);
    }

    public async Task<ServiceResult<MediaFileDto>> GetByIdAsync(
        IdRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediaFileService.GetByIdAsync(request, cancellationToken);
        if (result.Data is null)
        {
            return result;
        }

        if (!IsDocument(result.Data))
        {
            return ServiceResult<MediaFileDto>.NotFound("Doküman bulunamadı.");
        }

        return result;
    }

    public Task<ServiceResult<UploadMediaFileResultDto>> UploadAsync(
        UploadMediaFileDto request,
        CancellationToken cancellationToken = default)
    {
        request.FeatureType = "Document";
        if (string.IsNullOrWhiteSpace(request.MediaType))
        {
            request.MediaType = "Document";
        }

        return _mediaFileService.UploadAsync(request, cancellationToken);
    }

    public async Task<ServiceResult> UpdateAsync(
        UpdateMediaFileDto request,
        CancellationToken cancellationToken = default)
    {
        var existing = await _mediaFileService.GetByIdAsync(
            new IdRequest { Id = request.MediaFileId },
            cancellationToken);
        if (existing.Data is null)
        {
            return ServiceResult.NotFound("Doküman bulunamadı.");
        }

        if (!IsDocument(existing.Data))
        {
            return ServiceResult.NotFound("Doküman bulunamadı.");
        }

        return await _mediaFileService.UpdateAsync(request, cancellationToken);
    }

    public async Task<ServiceResult> DeleteAsync(
        IdRequest request,
        CancellationToken cancellationToken = default)
    {
        var existing = await _mediaFileService.GetByIdAsync(request, cancellationToken);
        if (existing.Data is null)
        {
            return ServiceResult.NotFound("Doküman bulunamadı.");
        }

        if (!IsDocument(existing.Data))
        {
            return ServiceResult.NotFound("Doküman bulunamadı.");
        }

        return await _mediaFileService.DeleteAsync(request, cancellationToken);
    }

    public async Task<ServiceResult<MediaFileDownloadDto>> OpenDownloadAsync(
        IdRequest request,
        CancellationToken cancellationToken = default)
    {
        var existing = await _mediaFileService.GetByIdAsync(request, cancellationToken);
        if (existing.Data is null)
        {
            return ServiceResult<MediaFileDownloadDto>.NotFound("Doküman bulunamadı.");
        }

        if (!IsDocument(existing.Data))
        {
            return ServiceResult<MediaFileDownloadDto>.NotFound("Doküman bulunamadı.");
        }

        return await _mediaFileService.OpenDownloadAsync(request, cancellationToken);
    }

    private static bool IsDocument(MediaFileDto dto) =>
        dto.MediaType.Equals("Document", StringComparison.OrdinalIgnoreCase);
}
