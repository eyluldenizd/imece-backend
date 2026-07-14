using Application.DTOs;
using Core.Common;
using Infrastructure.Entities;
using Infrastructure.Repositories;

namespace Application.Services;

public sealed class MediaFileService
{
    private static readonly HashSet<string> AllowedMediaTypes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "Photo",
            "Video",
            "Document"
        };

    private readonly MediaFileRepository _mediaFileRepository;
    private readonly MediaFolderRepository _mediaFolderRepository;

    public MediaFileService(
        MediaFileRepository mediaFileRepository,
        MediaFolderRepository mediaFolderRepository)
    {
        _mediaFileRepository = mediaFileRepository;
        _mediaFolderRepository = mediaFolderRepository;
    }

    public async Task<
        ServiceResult<IReadOnlyList<MediaFileDto>>>
        GetAllAsync(
            CancellationToken cancellationToken = default)
    {
        var files = await _mediaFileRepository.GetAllAsync(
            cancellationToken);

        IReadOnlyList<MediaFileDto> response = files
            .Select(ToDto)
            .ToList();

        return ServiceResult<IReadOnlyList<MediaFileDto>>
            .Success(response);
    }

    public async Task<
        ServiceResult<IReadOnlyList<MediaFileDto>>>
        GetActiveAsync(
            CancellationToken cancellationToken = default)
    {
        var files = await _mediaFileRepository.GetActiveAsync(
            cancellationToken);

        IReadOnlyList<MediaFileDto> response = files
            .Select(ToDto)
            .ToList();

        return ServiceResult<IReadOnlyList<MediaFileDto>>
            .Success(response);
    }

    public async Task<ServiceResult<MediaFileDto>> GetByIdAsync(
        IdRequest request,
        CancellationToken cancellationToken = default)
    {
        var file = await _mediaFileRepository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (file is null)
        {
            return ServiceResult<MediaFileDto>.NotFound(
                $"ID değeri {request.Id} olan medya dosyası bulunamadı.");
        }

        return ServiceResult<MediaFileDto>.Success(
            ToDto(file));
    }

    public async Task<
        ServiceResult<IReadOnlyList<MediaFileDto>>>
        GetByCompanyAsync(
            MediaFileCompanyRequest request,
            CancellationToken cancellationToken = default)
    {
        var files = await _mediaFileRepository.GetByCompanyAsync(
            request.CompanyId,
            cancellationToken);

        IReadOnlyList<MediaFileDto> response = files
            .Select(ToDto)
            .ToList();

        return ServiceResult<IReadOnlyList<MediaFileDto>>
            .Success(response);
    }

    public async Task<
        ServiceResult<IReadOnlyList<MediaFileDto>>>
        GetByFolderAsync(
            MediaFileFolderRequest request,
            CancellationToken cancellationToken = default)
    {
        var folder = await _mediaFolderRepository.GetByIdAsync(
            request.FolderId,
            cancellationToken);

        if (folder is null)
        {
            return ServiceResult<
                IReadOnlyList<MediaFileDto>>.NotFound(
                    $"ID değeri {request.FolderId} olan medya klasörü bulunamadı.");
        }

        var files = await _mediaFileRepository.GetByFolderAsync(
            request.FolderId,
            cancellationToken);

        IReadOnlyList<MediaFileDto> response = files
            .Select(ToDto)
            .ToList();

        return ServiceResult<IReadOnlyList<MediaFileDto>>
            .Success(response);
    }

    public async Task<
        ServiceResult<IReadOnlyList<MediaFileDto>>>
        GetByMediaTypeAsync(
            MediaFileTypeRequest request,
            CancellationToken cancellationToken = default)
    {
        var normalizedMediaType =
            NormalizeMediaType(request.MediaType);

        if (normalizedMediaType is null)
        {
            return ServiceResult<
                IReadOnlyList<MediaFileDto>>.BadRequest(
                    "Medya türü Photo, Video veya Document olmalıdır.");
        }

        var files =
            await _mediaFileRepository.GetByMediaTypeAsync(
                normalizedMediaType,
                cancellationToken);

        IReadOnlyList<MediaFileDto> response = files
            .Select(ToDto)
            .ToList();

        return ServiceResult<IReadOnlyList<MediaFileDto>>
            .Success(response);
    }

    public async Task<
        ServiceResult<IReadOnlyList<MediaFileDto>>>
        SearchAsync(
            MediaFileSearchRequest request,
            CancellationToken cancellationToken = default)
    {
        var searchText = request.SearchText.Trim();

        if (string.IsNullOrWhiteSpace(searchText))
        {
            return ServiceResult<
                IReadOnlyList<MediaFileDto>>.BadRequest(
                    "Arama metni boş olamaz.");
        }

        var files = await _mediaFileRepository.SearchAsync(
            searchText,
            cancellationToken);

        IReadOnlyList<MediaFileDto> response = files
            .Select(ToDto)
            .ToList();

        return ServiceResult<IReadOnlyList<MediaFileDto>>
            .Success(response);
    }

    public async Task<ServiceResult> UpdateAsync(
        UpdateMediaFileDto request,
        CancellationToken cancellationToken = default)
    {
        var existingFile =
            await _mediaFileRepository.GetByIdAsync(
                request.MediaFileId,
                cancellationToken);

        if (existingFile is null)
        {
            return ServiceResult.NotFound(
                $"ID değeri {request.MediaFileId} olan medya dosyası bulunamadı.");
        }

        if (request.FolderId.HasValue)
        {
            var folder =
                await _mediaFolderRepository.GetByIdAsync(
                    request.FolderId.Value,
                    cancellationToken);

            if (folder is null)
            {
                return ServiceResult.NotFound(
                    $"ID değeri {request.FolderId.Value} olan medya klasörü bulunamadı.");
            }

            if (!folder.IsActive)
            {
                return ServiceResult.Conflict(
                    "Dosya pasif bir klasöre taşınamaz.");
            }

            if (folder.CompanyId != existingFile.CompanyId)
            {
                return ServiceResult.BadRequest(
                    "Medya dosyası ile hedef klasör aynı şirkete ait olmalıdır.");
            }

            if (!IsMediaTypeCompatibleWithFolder(
                    existingFile.MediaType,
                    folder.FolderType))
            {
                return ServiceResult.BadRequest(
                    "Medya dosyasının türü hedef klasör türüyle uyumlu değildir.");
            }
        }

        if (request.SortOrder < 0)
        {
            return ServiceResult.BadRequest(
                "Sıralama değeri negatif olamaz.");
        }

        if (request.DurationSeconds.HasValue &&
            request.DurationSeconds.Value < 0)
        {
            return ServiceResult.BadRequest(
                "Video süresi negatif olamaz.");
        }

        if (request.ExpiryDate.HasValue &&
            request.EffectiveDate.HasValue &&
            request.ExpiryDate.Value <
            request.EffectiveDate.Value)
        {
            return ServiceResult.BadRequest(
                "Doküman geçerlilik bitiş tarihi, başlangıç tarihinden önce olamaz.");
        }

        var entity = new MediaFiles
        {
            MediaFileId = existingFile.MediaFileId,
            CompanyId = existingFile.CompanyId,
            FolderId = request.FolderId,

            MediaType = existingFile.MediaType,

            Title = request.Title.Trim(),

            Description = NormalizeOptionalText(
                request.Description),

            OriginalFileName =
                existingFile.OriginalFileName,

            StoredFileName =
                existingFile.StoredFileName,

            FileExtension =
                existingFile.FileExtension,

            ContentType =
                existingFile.ContentType,

            RelativePath =
                existingFile.RelativePath,

            ThumbnailPath =
                existingFile.ThumbnailPath,

            FileSizeBytes =
                existingFile.FileSizeBytes,

            DurationSeconds =
                request.DurationSeconds,

            DocumentNumber =
                NormalizeOptionalText(
                    request.DocumentNumber),

            DocumentVersion =
                NormalizeOptionalText(
                    request.DocumentVersion),

            EffectiveDate =
                request.EffectiveDate,

            ExpiryDate =
                request.ExpiryDate,

            SortOrder =
                request.SortOrder,

            IsActive =
                request.IsActive,

            UploadedBy =
                existingFile.UploadedBy,

            UploadedAt =
                existingFile.UploadedAt,

            UpdatedAt =
                existingFile.UpdatedAt
        };

        var rowsAffected =
            await _mediaFileRepository.UpdateAsync(
                entity,
                cancellationToken);

        if (rowsAffected == 0)
        {
            return ServiceResult.Conflict(
                "Medya dosyası güncellenemedi.");
        }

        return ServiceResult.NoContent();
    }

    public async Task<ServiceResult> DeleteAsync(
        IdRequest request,
        CancellationToken cancellationToken = default)
    {
        var file = await _mediaFileRepository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (file is null)
        {
            return ServiceResult.NotFound(
                $"ID değeri {request.Id} olan medya dosyası bulunamadı.");
        }

        if (!file.IsActive)
        {
            return ServiceResult.Conflict(
                "Medya dosyası zaten pasif durumdadır.");
        }

        var rowsAffected =
            await _mediaFileRepository.SoftDeleteAsync(
                request.Id,
                cancellationToken);

        if (rowsAffected == 0)
        {
            return ServiceResult.Conflict(
                "Medya dosyası pasif hale getirilemedi.");
        }

        return ServiceResult.NoContent();
    }

    private static MediaFileDto ToDto(
        MediaFileDetails entity)
    {
        return new MediaFileDto
        {
            MediaFileId =
                entity.MediaFileId,

            CompanyId =
                entity.CompanyId,

            FolderId =
                entity.FolderId,

            FolderName =
                entity.FolderName,

            MediaType =
                entity.MediaType,

            Title =
                entity.Title,

            Description =
                entity.Description,

            OriginalFileName =
                entity.OriginalFileName,

            StoredFileName =
                entity.StoredFileName,

            FileExtension =
                entity.FileExtension,

            ContentType =
                entity.ContentType,

            RelativePath =
                entity.RelativePath,

            ThumbnailPath =
                entity.ThumbnailPath,

            FileSizeBytes =
                entity.FileSizeBytes,

            DurationSeconds =
                entity.DurationSeconds,

            DocumentNumber =
                entity.DocumentNumber,

            DocumentVersion =
                entity.DocumentVersion,

            EffectiveDate =
                entity.EffectiveDate,

            ExpiryDate =
                entity.ExpiryDate,

            SortOrder =
                entity.SortOrder,

            IsActive =
                entity.IsActive,

            UploadedBy =
                entity.UploadedBy,

            UploadedByFullName =
                entity.UploadedByFullName,

            UploadedAt =
                entity.UploadedAt,

            UpdatedAt =
                entity.UpdatedAt
        };
    }

    private static string? NormalizeMediaType(
        string mediaType)
    {
        return AllowedMediaTypes
            .FirstOrDefault(type =>
                type.Equals(
                    mediaType.Trim(),
                    StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsMediaTypeCompatibleWithFolder(
        string mediaType,
        string folderType)
    {
        if (folderType.Equals(
                "MixedFolder",
                StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return mediaType.Trim().ToLowerInvariant() switch
        {
            "photo" => folderType.Equals(
                "PhotoAlbum",
                StringComparison.OrdinalIgnoreCase),

            "video" => folderType.Equals(
                "VideoAlbum",
                StringComparison.OrdinalIgnoreCase),

            "document" => folderType.Equals(
                "DocumentFolder",
                StringComparison.OrdinalIgnoreCase),

            _ => false
        };
    }

    private static string? NormalizeOptionalText(
        string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}