using Application.Common.CompanyScope;
using Application.Common.OrganizationScope;
using Application.Common.Storage;
using Application.DTOs;
using Core.Authorization;
using Core.Common;
using Core.Common.Messages;
using Infrastructure.Entities;
using Infrastructure.Repositories;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace Application.Services;

public sealed class MediaFileService
{
    private static readonly IReadOnlyDictionary<string, string> ExtensionMediaTypes =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [".jpg"] = "Photo", [".jpeg"] = "Photo", [".png"] = "Photo",
            [".webp"] = "Photo", [".gif"] = "Photo", [".mp4"] = "Video",
            [".pdf"] = "Document", [".doc"] = "Document", [".docx"] = "Document",
            [".xls"] = "Document", [".xlsx"] = "Document"
        };

    private static readonly HashSet<string> AllowedMediaTypes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "Photo",
            "Video",
            "Document"
        };

    private readonly MediaFileRepository _mediaFileRepository;
    private readonly MediaFolderRepository _mediaFolderRepository;
    private readonly CompanyRepository _companyRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly ICurrentUser _currentUser;
    private readonly ICompanyContext _companyContext;
    private readonly OrganizationScopeService _organizationScopeService;
    private readonly FileStorageOptions _fileStorageOptions;

    public MediaFileService(
        MediaFileRepository mediaFileRepository,
        MediaFolderRepository mediaFolderRepository,
        CompanyRepository companyRepository,
        IFileStorageService fileStorageService,
        ICurrentUser currentUser,
        ICompanyContext companyContext,
        OrganizationScopeService organizationScopeService,
        IOptions<FileStorageOptions> fileStorageOptions)
    {
        _mediaFileRepository = mediaFileRepository;
        _mediaFolderRepository = mediaFolderRepository;
        _companyRepository = companyRepository;
        _fileStorageService = fileStorageService;
        _currentUser = currentUser;
        _companyContext = companyContext;
        _organizationScopeService = organizationScopeService;
        _fileStorageOptions = fileStorageOptions.Value;
    }

    public async Task<ServiceResult<UploadMediaFileResultDto>> UploadAsync(
        UploadMediaFileDto request,
        CancellationToken cancellationToken = default)
    {
        // ResolveScope throws ForbiddenException (403) for unauthorized company / missing permission.
        // Invalid scope shape (e.g. Company without companyId) is also Forbidden by design.
        var scope = CompanyScopeRules.ResolveScope(
            _companyContext,
            _currentUser,
            request.ScopeType,
            request.CompanyId);

        if (!TryParseFeatureType(request.FeatureType, out var featureType, out var featureTypeError))
        {
            return ServiceResult<UploadMediaFileResultDto>.BadRequest(featureTypeError!);
        }

        var uploadedBy = _currentUser.GetRequiredUserId();

        if (request.File is null)
            return ServiceResult<UploadMediaFileResultDto>.BadRequest(MediaFileMessages.FileRequired);
        if (request.File.Length == 0)
            return ServiceResult<UploadMediaFileResultDto>.BadRequest(MediaFileMessages.FileEmpty);
        if (request.File.Length > _fileStorageOptions.MaxFileSizeBytes)
            return ServiceResult<UploadMediaFileResultDto>.BadRequest(MediaFileMessages.FileTooLarge);

        var mediaType = NormalizeMediaType(request.MediaType);
        if (mediaType is null)
            return ServiceResult<UploadMediaFileResultDto>.BadRequest(MediaFileMessages.MediaTypeInvalid);

        var originalFileName = Path.GetFileName(request.File.FileName);
        var extension = Path.GetExtension(originalFileName).ToLowerInvariant();
        if (!ExtensionMediaTypes.TryGetValue(extension, out var extensionMediaType))
            return ServiceResult<UploadMediaFileResultDto>.BadRequest(MediaFileMessages.UnsupportedExtension);
        if (!extensionMediaType.Equals(mediaType, StringComparison.OrdinalIgnoreCase))
            return ServiceResult<UploadMediaFileResultDto>.BadRequest(MediaFileMessages.FileMediaTypeMismatch);

        if (request.FolderId.HasValue)
        {
            if (scope.ScopeType.Equals(ContentScopeTypes.Global, StringComparison.OrdinalIgnoreCase))
            {
                return ServiceResult<UploadMediaFileResultDto>.BadRequest(
                    "Global kapsamlı yüklemeler klasör ile ilişkilendirilemez.");
            }

            var folder = await _mediaFolderRepository.GetByIdAsync(
                request.FolderId.Value, cancellationToken);
            if (folder is null)
                return ServiceResult<UploadMediaFileResultDto>.NotFound(
                    MediaFileMessages.FolderNotFound(request.FolderId.Value));
            CompanyScopeRules.EnsureCompanyAccess(_companyContext, folder.CompanyId);
            if (!folder.IsActive)
                return ServiceResult<UploadMediaFileResultDto>.Conflict(MediaFileMessages.FolderInactive);
            if (folder.CompanyId != scope.CompanyId)
                return ServiceResult<UploadMediaFileResultDto>.BadRequest(MediaFileMessages.FolderCompanyMismatch);
            if (!IsMediaTypeCompatibleWithFolder(mediaType, folder.FolderType))
                return ServiceResult<UploadMediaFileResultDto>.BadRequest(MediaFileMessages.FolderMediaTypeMismatch);
        }

        if (request.SortOrder < 0)
            return ServiceResult<UploadMediaFileResultDto>.BadRequest(MediaFileMessages.SortOrderInvalid);
        if (request.DurationSeconds is < 0)
            return ServiceResult<UploadMediaFileResultDto>.BadRequest(MediaFileMessages.DurationInvalid);
        if (request.ExpiryDate.HasValue && request.EffectiveDate.HasValue &&
            request.ExpiryDate.Value < request.EffectiveDate.Value)
            return ServiceResult<UploadMediaFileResultDto>.BadRequest(MediaFileMessages.DateRangeInvalid);

        var orgScopeResult = await ResolveUploadOrganizationScopeAsync(
            request,
            scope,
            cancellationToken);
        if (orgScopeResult.ErrorMessage is not null)
        {
            return ServiceResult<UploadMediaFileResultDto>.BadRequest(orgScopeResult.ErrorMessage);
        }

        string? companyName = null;
        if (scope.CompanyId.HasValue)
        {
            var company = await _companyRepository.GetByIdAsync(
                scope.CompanyId.Value,
                cancellationToken);
            if (company is null || !company.IsActive)
            {
                return ServiceResult<UploadMediaFileResultDto>.NotFound(
                    "Hedef şirket bulunamadı veya pasif.");
            }

            companyName = company.CompanyName;
        }

        StoredFileResult? storedFile = null;

        try
        {
            await using var content = request.File.OpenReadStream();
            storedFile = await _fileStorageService.SaveAsync(
                new FileStorageRequest
                {
                    Content = content,
                    OriginalFileName = originalFileName,
                    FeatureType = featureType,
                    ScopeType = ParseStorageScopeType(scope.ScopeType),
                    CompanyId = scope.CompanyId,
                    CompanySlug = companyName,
                    ContentLength = request.File.Length
                },
                cancellationToken);

            var entity = new MediaFiles
            {
                CompanyId = scope.CompanyId,
                ScopeType = scope.ScopeType,
                BranchScope = orgScopeResult.BranchScope!,
                BranchId = orgScopeResult.BranchId,
                DepartmentScope = orgScopeResult.DepartmentScope!,
                DepartmentId = orgScopeResult.DepartmentId,
                FolderId = request.FolderId,
                MediaType = mediaType,
                Title = request.Title.Trim(),
                Description = NormalizeOptionalText(request.Description),
                OriginalFileName = originalFileName,
                StoredFileName = storedFile.StoredFileName,
                FileExtension = storedFile.FileExtension,
                ContentType = string.IsNullOrWhiteSpace(request.File.ContentType)
                    ? "application/octet-stream"
                    : request.File.ContentType,
                RelativePath = storedFile.PublicRelativeUrl,
                FileSizeBytes = request.File.Length,
                DurationSeconds = request.DurationSeconds,
                DocumentNumber = NormalizeOptionalText(request.DocumentNumber),
                DocumentVersion = NormalizeOptionalText(request.DocumentVersion),
                EffectiveDate = request.EffectiveDate,
                ExpiryDate = request.ExpiryDate,
                SortOrder = request.SortOrder,
                IsActive = true,
                UploadedBy = uploadedBy
            };

            var mediaFileId = await _mediaFileRepository.CreateAsync(
                entity, cancellationToken);

            return ServiceResult<UploadMediaFileResultDto>.Created(
                new UploadMediaFileResultDto
                {
                    MediaFileId = mediaFileId,
                    RelativeUrl = storedFile.PublicRelativeUrl
                });
        }
        catch (InvalidOperationException ex)
        {
            await TryDeleteStoredFileAsync(storedFile);
            return ServiceResult<UploadMediaFileResultDto>.BadRequest(ex.Message);
        }
        catch (SqlException ex)
        {
            await TryDeleteStoredFileAsync(storedFile);
            return ServiceResult<UploadMediaFileResultDto>.BadRequest(MapSqlUploadError(ex));
        }
        catch
        {
            await TryDeleteStoredFileAsync(storedFile);
            throw;
        }
    }

    private async Task TryDeleteStoredFileAsync(StoredFileResult? storedFile)
    {
        if (storedFile is null)
        {
            return;
        }

        try
        {
            await _fileStorageService.DeleteAsync(
                storedFile.PublicRelativeUrl,
                CancellationToken.None);
        }
        catch
        {
            // Best-effort cleanup; do not mask the original upload error.
        }
    }

    private static string MapSqlUploadError(SqlException exception) =>
        exception.Number switch
        {
            547 => "Medya dosyası kaydedilemedi: kapsam (scope_type/company_id) veya ilişkili kayıt kısıtı ihlal edildi.",
            515 => "Medya dosyası kaydedilemedi: zorunlu alan eksik (company_id veya scope_type uyumsuz olabilir).",
            2627 or 2601 => "Medya dosyası kaydedilemedi: benzersizlik ihlali.",
            _ => $"Medya dosyası kaydedilemedi: {exception.Message}"
        };

    public async Task<
        ServiceResult<IReadOnlyList<MediaFileDto>>>
        GetAllAsync(
            CancellationToken cancellationToken = default)
    {
        var files = await _mediaFileRepository.GetAllAsync(
            CompanyScopeRules.ResolveListCompanyFilter(_companyContext, _currentUser),
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
            CompanyScopeRules.ResolveListCompanyFilter(_companyContext, _currentUser),
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

        CompanyScopeRules.EnsureContentReadAccess(_companyContext, file.ScopeType, file.CompanyId);

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

        CompanyScopeRules.EnsureContentWriteAccess(
            _companyContext,
            _currentUser,
            existingFile.ScopeType,
            existingFile.CompanyId);

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

        CompanyScopeRules.EnsureContentWriteAccess(
            _companyContext,
            _currentUser,
            file.ScopeType,
            file.CompanyId);

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

            ScopeType =
                entity.ScopeType,

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

    private static bool TryParseFeatureType(
        string? value,
        out StorageFeatureType featureType,
        out string? errorMessage)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            featureType = StorageFeatureType.Media;
            errorMessage = null;
            return true;
        }

        if (Enum.TryParse<StorageFeatureType>(value, true, out featureType))
        {
            errorMessage = null;
            return true;
        }

        featureType = default;
        errorMessage = MediaFileMessages.FeatureTypeInvalid;
        return false;
    }

    private static StorageScopeType ParseStorageScopeType(string scopeType) =>
        scopeType.Equals(ContentScopeTypes.Global, StringComparison.OrdinalIgnoreCase)
            ? StorageScopeType.Global
            : StorageScopeType.Company;

    private async Task<(string? BranchScope, int? BranchId, string? DepartmentScope, int? DepartmentId, string? ErrorMessage)>
        ResolveUploadOrganizationScopeAsync(
            UploadMediaFileDto request,
            ResolvedContentScope contentScope,
            CancellationToken cancellationToken)
    {
        var hasBranch = request.BranchId.HasValue && request.BranchId.Value > 0;
        var hasDepartment = request.DepartmentId.HasValue && request.DepartmentId.Value > 0;
        var branchSpecific = OrganizationScopeFieldHelper.ParseLevel(request.BranchScope) == OrganizationScopeLevel.Specific;
        var departmentSpecific = OrganizationScopeFieldHelper.ParseLevel(request.DepartmentScope) == OrganizationScopeLevel.Specific;

        if (!hasBranch && !hasDepartment && !branchSpecific && !departmentSpecific)
        {
            return (OrganizationScopeFieldHelper.All, null, OrganizationScopeFieldHelper.All, null, null);
        }

        if (contentScope.ScopeType.Equals(ContentScopeTypes.Global, StringComparison.OrdinalIgnoreCase)
            || !contentScope.CompanyId.HasValue)
        {
            return (null, null, null, null,
                "Şube/departman kapsamı yalnızca şirket kapsamlı yüklemelerde kullanılabilir.");
        }

        try
        {
            var orgScope = new OrganizationScope
            {
                CompanyScope = OrganizationScopeLevel.Specific,
                CompanyId = contentScope.CompanyId,
                BranchScope = hasBranch || branchSpecific
                    ? OrganizationScopeLevel.Specific
                    : OrganizationScopeLevel.All,
                BranchId = hasBranch ? request.BranchId : null,
                DepartmentScope = hasDepartment || departmentSpecific
                    ? OrganizationScopeLevel.Specific
                    : OrganizationScopeLevel.All,
                DepartmentId = hasDepartment ? request.DepartmentId : null
            };

            var resolved = await _organizationScopeService.ResolveAsync(
                new OrganizationScopeFieldsDto
                {
                    CompanyScope = OrganizationScopeFieldHelper.FormatLevel(orgScope.CompanyScope),
                    CompanyId = orgScope.CompanyId,
                    BranchScope = OrganizationScopeFieldHelper.FormatLevel(orgScope.BranchScope),
                    BranchId = orgScope.BranchId,
                    DepartmentScope = OrganizationScopeFieldHelper.FormatLevel(orgScope.DepartmentScope),
                    DepartmentId = orgScope.DepartmentId
                },
                cancellationToken);

            if (resolved.ErrorMessage is not null)
            {
                return (null, null, null, null, resolved.ErrorMessage);
            }

            return (
                OrganizationScopeFieldHelper.FormatLevel(resolved.Resolved!.BranchScope),
                resolved.Resolved.BranchId,
                OrganizationScopeFieldHelper.FormatLevel(resolved.Resolved.DepartmentScope),
                resolved.Resolved.DepartmentId,
                null);
        }
        catch (InvalidOperationException ex)
        {
            return (null, null, null, null, ex.Message);
        }
    }
}
