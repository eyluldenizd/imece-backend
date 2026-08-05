using Application.Common.ListQuery;
using Application.DTOs;
using Core.Authorization;
using Core.Common;
using Infrastructure.Repositories;

namespace Application.Services;

/// <summary>
/// Permission'lar sistem catalog capability'leridir.
/// Serbest create/delete kapalıdır; yalnız description metadata güncellenebilir.
/// </summary>
public sealed class PermissionService
{
    private readonly PermissionRepository _permissionRepository;

    public PermissionService(PermissionRepository permissionRepository)
    {
        _permissionRepository = permissionRepository;
    }

    public async Task<ServiceResult<IReadOnlyList<PermissionDto>>> GetAllAsync(
        ContentListQueryDto? query = null,
        CancellationToken cancellationToken = default)
    {
        var permissions = await _permissionRepository.GetAllAsync(cancellationToken);
        var roleCounts = await _permissionRepository.GetAssignedRoleCountsAsync(cancellationToken);
        var countById = roleCounts.ToDictionary(row => row.PermissionId, row => row.RoleCount);

        var items = permissions
            .Select(entity => ToDto(entity, countById.GetValueOrDefault(entity.PermissionId)))
            .OrderBy(p => p.PermissionCode, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return ServiceResult<IReadOnlyList<PermissionDto>>.Success(
            AdminListQueryProfiles.ApplyToPermissions(items, query));
    }

    public async Task<ServiceResult<PermissionDto>> GetByIdAsync(
        IdRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _permissionRepository.GetByIdAsync((int)request.Id, cancellationToken);
        if (entity is null)
        {
            return ServiceResult<PermissionDto>.NotFound(
                $"ID değeri {request.Id} olan yetki bulunamadı.");
        }

        var assignedRoleCount = await _permissionRepository.GetAssignedRoleCountAsync(
            entity.PermissionId,
            cancellationToken);

        return ServiceResult<PermissionDto>.Success(ToDto(entity, assignedRoleCount));
    }

    public Task<ServiceResult<int>> CreateAsync(
        CreatePermissionDto request,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(ServiceResult<int>.Conflict(
            "Yetkiler sistem catalog'undan gelir. Serbest yetki oluşturma kapalıdır. " +
            "SystemPermissionCatalog genişletilmeli ve seed çalıştırılmalıdır."));
    }

    public async Task<ServiceResult> UpdateAsync(
        UpdatePermissionDto request,
        CancellationToken cancellationToken = default)
    {
        var existing = await _permissionRepository.GetByIdAsync(request.PermissionId, cancellationToken);
        if (existing is null)
        {
            return ServiceResult.NotFound(
                $"ID değeri {request.PermissionId} olan yetki bulunamadı.");
        }

        if (!SystemPermissionCatalog.IsSystemPermission(existing.PermissionCode))
        {
            return ServiceResult.Conflict(
                "Catalog dışı yetki kayıtları desteklenmez. Seed ile senkronize edin.");
        }

        // Code immutable; only description metadata may change.
        if (!string.IsNullOrWhiteSpace(request.PermissionCode)
            && !string.Equals(
                request.PermissionCode.Trim(),
                existing.PermissionCode,
                StringComparison.OrdinalIgnoreCase))
        {
            return ServiceResult.Conflict("Sistem yetkisinin kodu değiştirilemez.");
        }

        existing.Description = string.IsNullOrWhiteSpace(request.Description)
            ? existing.Description
            : request.Description.Trim();

        var catalog = SystemPermissionCatalog.Find(existing.PermissionCode);
        if (catalog is not null && string.IsNullOrWhiteSpace(request.Description))
        {
            existing.Description = catalog.Description;
        }

        var rows = await _permissionRepository.UpdateAsync(existing, cancellationToken);
        if (rows == 0)
        {
            return ServiceResult.Conflict("Yetki güncellenemedi.");
        }

        return ServiceResult.NoContent();
    }

    public Task<ServiceResult> DeleteAsync(
        IdRequest request,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(ServiceResult.Conflict(
            "Yetkiler sistem catalog'undan gelir ve silinemez."));
    }

    private static PermissionDto ToDto(PermissionRecord entity, int assignedRoleCount) => new()
    {
        PermissionId = entity.PermissionId,
        PermissionCode = entity.PermissionCode,
        Description = entity.Description,
        IsSystem = SystemPermissionCatalog.IsSystemPermission(entity.PermissionCode),
        AssignedRoleCount = assignedRoleCount
    };
}
