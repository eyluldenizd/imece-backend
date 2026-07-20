using Application.Common.CompanyScope;
using Application.DTOs;
using Core.Authorization;
using Core.Common;
using Infrastructure.Entities;
using Infrastructure.Repositories;

namespace Application.Services;

public sealed class MediaFolderService
{
    private static readonly HashSet<string> AllowedFolderTypes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "PhotoAlbum",
            "VideoAlbum",
            "DocumentFolder",
            "MixedFolder"
        };

    private readonly MediaFolderRepository _mediaFolderRepository;
    private readonly MediaFileRepository _mediaFileRepository;
    private readonly ICurrentUser _currentUser;
    private readonly ICompanyContext _companyContext;

    public MediaFolderService(
        MediaFolderRepository mediaFolderRepository,
        MediaFileRepository mediaFileRepository,
        ICurrentUser currentUser,
        ICompanyContext companyContext)
    {
        _mediaFolderRepository = mediaFolderRepository;
        _mediaFileRepository = mediaFileRepository;
        _currentUser = currentUser;
        _companyContext = companyContext;
    }

    public async Task<
        ServiceResult<IReadOnlyList<MediaFolderDto>>>
        GetAllAsync(
            CancellationToken cancellationToken = default)
    {
        var folders = await _mediaFolderRepository.GetAllAsync(
            CompanyScopeRules.ResolveListCompanyFilter(_companyContext, _currentUser),
            cancellationToken);

        IReadOnlyList<MediaFolderDto> response = folders
            .Select(ToDto)
            .ToList();

        return ServiceResult<IReadOnlyList<MediaFolderDto>>
            .Success(response);
    }

    public async Task<
        ServiceResult<IReadOnlyList<MediaFolderDto>>>
        GetActiveAsync(
            CancellationToken cancellationToken = default)
    {
        var folders = await _mediaFolderRepository.GetActiveAsync(
            CompanyScopeRules.ResolveListCompanyFilter(_companyContext, _currentUser),
            cancellationToken);

        IReadOnlyList<MediaFolderDto> response = folders
            .Select(ToDto)
            .ToList();

        return ServiceResult<IReadOnlyList<MediaFolderDto>>
            .Success(response);
    }

    public async Task<ServiceResult<MediaFolderDto>> GetByIdAsync(
        IdRequest request,
        CancellationToken cancellationToken = default)
    {
        var folder = await _mediaFolderRepository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (folder is null)
        {
            return ServiceResult<MediaFolderDto>.NotFound(
                $"ID değeri {request.Id} olan medya klasörü bulunamadı.");
        }

        CompanyScopeRules.EnsureCompanyAccess(_companyContext, folder.CompanyId);

        return ServiceResult<MediaFolderDto>.Success(
            ToDto(folder));
    }

    public async Task<
        ServiceResult<IReadOnlyList<MediaFolderDto>>>
        GetByCompanyAsync(
            MediaFolderCompanyRequest request,
            CancellationToken cancellationToken = default)
    {
        var folders =
            await _mediaFolderRepository.GetByCompanyAsync(
                request.CompanyId,
                cancellationToken);

        IReadOnlyList<MediaFolderDto> response = folders
            .Select(ToDto)
            .ToList();

        return ServiceResult<IReadOnlyList<MediaFolderDto>>
            .Success(response);
    }

    public async Task<
        ServiceResult<IReadOnlyList<MediaFolderDto>>>
        GetChildrenAsync(
            MediaFolderChildrenRequest request,
            CancellationToken cancellationToken = default)
    {
        var parentFolder =
            await _mediaFolderRepository.GetByIdAsync(
                request.ParentFolderId,
                cancellationToken);

        if (parentFolder is null)
        {
            return ServiceResult<
                IReadOnlyList<MediaFolderDto>>.NotFound(
                    $"ID değeri {request.ParentFolderId} olan üst klasör bulunamadı.");
        }

        var folders =
            await _mediaFolderRepository.GetChildrenAsync(
                request.ParentFolderId,
                cancellationToken);

        IReadOnlyList<MediaFolderDto> response = folders
            .Select(ToDto)
            .ToList();

        return ServiceResult<IReadOnlyList<MediaFolderDto>>
            .Success(response);
    }

    public async Task<ServiceResult<long>> CreateAsync(
        CreateMediaFolderDto request,
        CancellationToken cancellationToken = default)
    {
        if (request.CompanyId <= 0)
        {
            return ServiceResult<long>.BadRequest(
                "Klasör oluşturmak için geçerli bir şirket belirtilmelidir.");
        }

        _companyContext.EnsureCanAccessCompany(request.CompanyId);
        var companyId = request.CompanyId;
        var createdBy = _currentUser.GetRequiredUserId();

        var normalizedFolderType =
            NormalizeFolderType(request.FolderType);

        if (normalizedFolderType is null)
        {
            return ServiceResult<long>.BadRequest(
                "Klasör türü PhotoAlbum, VideoAlbum, DocumentFolder veya MixedFolder olmalıdır.");
        }

        if (request.ParentFolderId.HasValue)
        {
            var parentFolder =
                await _mediaFolderRepository.GetByIdAsync(
                    request.ParentFolderId.Value,
                    cancellationToken);

            if (parentFolder is null)
            {
                return ServiceResult<long>.NotFound(
                    $"ID değeri {request.ParentFolderId.Value} olan üst klasör bulunamadı.");
            }

            CompanyScopeRules.EnsureCompanyAccess(_companyContext, parentFolder.CompanyId);

            if (!parentFolder.IsActive)
            {
                return ServiceResult<long>.Conflict(
                    "Pasif bir klasörün altında yeni klasör oluşturulamaz.");
            }

            if (parentFolder.CompanyId != companyId)
            {
                return ServiceResult<long>.BadRequest(
                    "Üst klasör ile oluşturulan klasör aynı şirkete ait olmalıdır.");
            }
        }

        var entity = new MediaFolders
        {
            CompanyId = companyId,
            ParentFolderId = request.ParentFolderId,
            FolderName = request.FolderName.Trim(),
            FolderType = normalizedFolderType,
            Description = NormalizeOptionalText(
                request.Description),
            EventId = request.EventId,
            CoverMediaFileId = null,
            IsPublic = request.IsPublic,
            IsActive = true,
            CreatedBy = createdBy
        };

        var folderId =
            await _mediaFolderRepository.CreateAsync(
                entity,
                cancellationToken);

        return ServiceResult<long>.Created(folderId);
    }

    public async Task<ServiceResult> UpdateAsync(
        UpdateMediaFolderDto request,
        CancellationToken cancellationToken = default)
    {
        var existingFolder =
            await _mediaFolderRepository.GetByIdAsync(
                request.FolderId,
                cancellationToken);

        if (existingFolder is null)
        {
            return ServiceResult.NotFound(
                $"ID değeri {request.FolderId} olan medya klasörü bulunamadı.");
        }

        CompanyScopeRules.EnsureCompanyAccess(_companyContext, existingFolder.CompanyId);

        var normalizedFolderType =
            NormalizeFolderType(request.FolderType);

        if (normalizedFolderType is null)
        {
            return ServiceResult.BadRequest(
                "Klasör türü PhotoAlbum, VideoAlbum, DocumentFolder veya MixedFolder olmalıdır.");
        }

        if (request.ParentFolderId == request.FolderId)
        {
            return ServiceResult.BadRequest(
                "Bir klasör kendi üst klasörü olarak atanamaz.");
        }

        if (request.ParentFolderId.HasValue)
        {
            var parentFolder =
                await _mediaFolderRepository.GetByIdAsync(
                    request.ParentFolderId.Value,
                    cancellationToken);

            if (parentFolder is null)
            {
                return ServiceResult.NotFound(
                    $"ID değeri {request.ParentFolderId.Value} olan üst klasör bulunamadı.");
            }

            if (!parentFolder.IsActive)
            {
                return ServiceResult.Conflict(
                    "Pasif bir klasör üst klasör olarak atanamaz.");
            }

            if (parentFolder.CompanyId != existingFolder.CompanyId)
            {
                return ServiceResult.BadRequest(
                    "Üst klasör ile güncellenen klasör aynı şirkete ait olmalıdır.");
            }
        }

        if (request.CoverMediaFileId.HasValue)
        {
            var coverFile =
                await _mediaFileRepository.GetByIdAsync(
                    request.CoverMediaFileId.Value,
                    cancellationToken);

            if (coverFile is null)
            {
                return ServiceResult.NotFound(
                    $"ID değeri {request.CoverMediaFileId.Value} olan kapak dosyası bulunamadı.");
            }

            if (!coverFile.IsActive)
            {
                return ServiceResult.Conflict(
                    "Pasif bir medya dosyası kapak olarak atanamaz.");
            }

            if (coverFile.CompanyId != existingFolder.CompanyId)
            {
                return ServiceResult.BadRequest(
                    "Kapak dosyası ile klasör aynı şirkete ait olmalıdır.");
            }

            if (!coverFile.MediaType.Equals(
                    "Photo",
                    StringComparison.OrdinalIgnoreCase))
            {
                return ServiceResult.BadRequest(
                    "Klasör kapağı olarak yalnızca fotoğraf türündeki dosyalar kullanılabilir.");
            }
        }

        var entity = new MediaFolders
        {
            FolderId = request.FolderId,
            CompanyId = existingFolder.CompanyId,
            ParentFolderId = request.ParentFolderId,
            FolderName = request.FolderName.Trim(),
            FolderType = normalizedFolderType,
            Description = NormalizeOptionalText(
                request.Description),
            EventId = request.EventId,
            CoverMediaFileId = request.CoverMediaFileId,
            IsPublic = request.IsPublic,
            IsActive = request.IsActive,
            CreatedBy = existingFolder.CreatedBy
        };

        var rowsAffected =
            await _mediaFolderRepository.UpdateAsync(
                entity,
                cancellationToken);

        if (rowsAffected == 0)
        {
            return ServiceResult.Conflict(
                "Medya klasörü güncellenemedi.");
        }

        return ServiceResult.NoContent();
    }

    public async Task<ServiceResult> DeleteAsync(
        IdRequest request,
        CancellationToken cancellationToken = default)
    {
        var folder = await _mediaFolderRepository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (folder is null)
        {
            return ServiceResult.NotFound(
                $"ID değeri {request.Id} olan medya klasörü bulunamadı.");
        }

        CompanyScopeRules.EnsureCompanyAccess(_companyContext, folder.CompanyId);

        if (!folder.IsActive)
        {
            return ServiceResult.Conflict(
                "Medya klasörü zaten pasif durumdadır.");
        }

        var hasActiveChildren =
            await _mediaFolderRepository.HasActiveChildrenAsync(
                request.Id,
                cancellationToken);

        if (hasActiveChildren)
        {
            return ServiceResult.Conflict(
                "Alt klasörleri bulunan bir klasör pasif hale getirilemez.");
        }

        var hasActiveFiles =
            await _mediaFolderRepository.HasActiveFilesAsync(
                request.Id,
                cancellationToken);

        if (hasActiveFiles)
        {
            return ServiceResult.Conflict(
                "İçinde aktif dosyalar bulunan bir klasör pasif hale getirilemez.");
        }

        var rowsAffected =
            await _mediaFolderRepository.SoftDeleteAsync(
                request.Id,
                cancellationToken);

        if (rowsAffected == 0)
        {
            return ServiceResult.Conflict(
                "Medya klasörü pasif hale getirilemedi.");
        }

        return ServiceResult.NoContent();
    }

    private static MediaFolderDto ToDto(
        MediaFolderDetails entity)
    {
        return new MediaFolderDto
        {
            FolderId = entity.FolderId,
            CompanyId = entity.CompanyId,
            ParentFolderId = entity.ParentFolderId,
            ParentFolderName = entity.ParentFolderName,
            FolderName = entity.FolderName,
            FolderType = entity.FolderType,
            Description = entity.Description,
            EventId = entity.EventId,
            EventTitle = entity.EventTitle,
            CoverMediaFileId = entity.CoverMediaFileId,
            CoverMediaPath = entity.CoverMediaPath,
            IsPublic = entity.IsPublic,
            IsActive = entity.IsActive,
            CreatedBy = entity.CreatedBy,
            CreatedByFullName = entity.CreatedByFullName,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    private static string? NormalizeFolderType(
        string folderType)
    {
        var matchingType = AllowedFolderTypes
            .FirstOrDefault(type =>
                type.Equals(
                    folderType.Trim(),
                    StringComparison.OrdinalIgnoreCase));

        return matchingType;
    }

    private static string? NormalizeOptionalText(
        string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}