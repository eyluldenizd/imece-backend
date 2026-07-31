using Application.Common.ListQuery;
using Application.DTOs;
using Core.Authorization;
using Core.Common;
using Infrastructure.Repositories;
using RoleEntity = Infrastructure.Entities.Roles;

namespace Application.Services;

public sealed class RoleService
{
    private readonly RoleRepository _roleRepository;
    private readonly PermissionRepository _permissionRepository;

    public RoleService(
        RoleRepository roleRepository,
        PermissionRepository permissionRepository)
    {
        _roleRepository = roleRepository;
        _permissionRepository = permissionRepository;
    }

    public async Task<ServiceResult<IReadOnlyList<RoleListItemDto>>> GetAllAsync(
        ContentListQueryDto? query = null,
        CancellationToken cancellationToken = default)
    {
        var roles = await _roleRepository.GetAllAsync(cancellationToken);
        var items = roles
            .Select(ToListItemDto)
            .OrderByDescending(role => SystemRoleCatalog.IsSystemRole(role.RoleName))
            .ThenBy(role => role.RoleName, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return ServiceResult<IReadOnlyList<RoleListItemDto>>.Success(
            AdminListQueryProfiles.ApplyToRoles(items, query));
    }

    public async Task<ServiceResult<RoleDto>> GetByIdAsync(
        IdRequest request,
        CancellationToken cancellationToken = default)
    {
        var role = await _roleRepository.GetByIdAsync((int)request.Id, cancellationToken);
        if (role is null)
        {
            return ServiceResult<RoleDto>.NotFound(
                $"ID değeri {request.Id} olan rol bulunamadı.");
        }

        var permissionCodes = await _roleRepository.GetPermissionCodesByRoleIdAsync(
            role.RoleId,
            cancellationToken);

        return ServiceResult<RoleDto>.Success(
            ToDto(role, permissionCodes.Select(row => row.PermissionCode).ToList()));
    }

    public async Task<ServiceResult<int>> CreateAsync(
        CreateRoleDto request,
        CancellationToken cancellationToken = default)
    {
        var roleName = (request.RoleName ?? string.Empty).Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(roleName))
        {
            return ServiceResult<int>.BadRequest("Rol adı zorunludur.");
        }

        if (roleName.Length > 64)
        {
            return ServiceResult<int>.BadRequest("Rol adı en fazla 64 karakter olabilir.");
        }

        if (SystemRoleCatalog.IsSystemRole(roleName))
        {
            return ServiceResult<int>.BadRequest(
                "Sistem rol adları kullanılamaz. Farklı bir rol adı seçin.");
        }

        if (await _roleRepository.ExistsByNameAsync(roleName, null, cancellationToken))
        {
            return ServiceResult<int>.Conflict("Bu rol adı zaten kullanılıyor.");
        }

        var permissionIds = NormalizePermissionIds(request.PermissionIds);
        var permissionError = await ValidatePermissionIdsAsync(permissionIds, cancellationToken);
        if (permissionError is not null)
        {
            return ServiceResult<int>.BadRequest(permissionError);
        }

        var entity = new RoleEntity
        {
            RoleName = roleName,
            Description = string.IsNullOrWhiteSpace(request.Description)
                ? null
                : request.Description.Trim(),
            IsActive = request.IsActive
        };

        try
        {
            var roleId = await _roleRepository.CreateWithPermissionsAsync(
                entity,
                permissionIds,
                cancellationToken);
            return ServiceResult<int>.Created(roleId);
        }
        catch (Exception ex) when (IsUniqueConstraintViolation(ex))
        {
            return ServiceResult<int>.Conflict("Bu rol adı zaten kullanılıyor.");
        }
    }

    public async Task<ServiceResult> UpdateAsync(
        UpdateRoleDto request,
        CancellationToken cancellationToken = default)
    {
        var existing = await _roleRepository.GetByIdAsync(request.RoleId, cancellationToken);
        if (existing is null)
        {
            return ServiceResult.NotFound(
                $"ID değeri {request.RoleId} olan rol bulunamadı.");
        }

        if (SystemRoleCatalog.IsSystemRole(existing.RoleName))
        {
            return ServiceResult.Conflict(
                "Sistem rolünün adı ve durumu değiştirilemez. Yalnızca yetkiler güncellenebilir.");
        }

        var roleName = request.RoleName.Trim().ToLowerInvariant();
        if (SystemRoleCatalog.IsSystemRole(roleName))
        {
            return ServiceResult.BadRequest("Sistem rol adları kullanılamaz.");
        }

        if (await _roleRepository.ExistsByNameAsync(
                roleName,
                request.RoleId,
                cancellationToken))
        {
            return ServiceResult.Conflict("Bu rol adı zaten kullanılıyor.");
        }

        existing.RoleName = roleName;
        existing.Description = request.Description?.Trim();
        existing.IsActive = request.IsActive;

        try
        {
            var rows = await _roleRepository.UpdateAsync(existing, cancellationToken);
            if (rows == 0)
            {
                return ServiceResult.Conflict("Rol güncellenemedi.");
            }
        }
        catch (Exception ex) when (IsUniqueConstraintViolation(ex))
        {
            return ServiceResult.Conflict("Bu rol adı zaten kullanılıyor.");
        }

        return ServiceResult.NoContent();
    }

    private static bool IsUniqueConstraintViolation(Exception ex)
    {
        // Avoid hard dependency on Microsoft.Data.SqlClient in Application.
        var number = ex.GetType().GetProperty("Number")?.GetValue(ex) as int?;
        return number is 2601 or 2627
            || (ex.InnerException is not null && IsUniqueConstraintViolation(ex.InnerException));
    }

    public async Task<ServiceResult> DeleteAsync(
        IdRequest request,
        CancellationToken cancellationToken = default)
    {
        var existing = await _roleRepository.GetByIdAsync((int)request.Id, cancellationToken);
        if (existing is null)
        {
            return ServiceResult.NotFound(
                $"ID değeri {request.Id} olan rol bulunamadı.");
        }

        if (SystemRoleCatalog.IsSystemRole(existing.RoleName))
        {
            return ServiceResult.Conflict("Sistem rolleri silinemez veya pasife alınamaz.");
        }

        var assignmentCount = await _roleRepository.CountActiveAssignmentsAsync(
            existing.RoleId,
            cancellationToken);

        if (assignmentCount > 0)
        {
            return ServiceResult.Conflict(
                $"Bu role atanmış {assignmentCount} kullanıcı/kayıt var. " +
                "Önce kullanıcı atamalarını kaldırın veya rolü pasife alın.");
        }

        var rows = await _roleRepository.SoftDeleteAsync((int)request.Id, cancellationToken);
        if (rows == 0)
        {
            return ServiceResult.NotFound(
                $"ID değeri {request.Id} olan rol bulunamadı veya zaten pasif.");
        }

        return ServiceResult.NoContent();
    }

    public async Task<ServiceResult> UpdatePermissionsAsync(
        UpdateRolePermissionsRequest request,
        CancellationToken cancellationToken = default)
    {
        var role = await _roleRepository.GetByIdAsync(request.RoleId, cancellationToken);
        if (role is null)
        {
            return ServiceResult.NotFound(
                $"ID değeri {request.RoleId} olan rol bulunamadı.");
        }

        if (!role.IsActive)
        {
            return ServiceResult.BadRequest("Pasif role yetki atanamaz.");
        }

        var permissionIds = NormalizePermissionIds(request.PermissionIds);
        var permissionError = await ValidatePermissionIdsAsync(permissionIds, cancellationToken);
        if (permissionError is not null)
        {
            return ServiceResult.BadRequest(permissionError);
        }

        await _roleRepository.ReplacePermissionsAsync(
            request.RoleId,
            permissionIds,
            cancellationToken);

        return ServiceResult.NoContent();
    }

    private async Task<string?> ValidatePermissionIdsAsync(
        IReadOnlyList<int> permissionIds,
        CancellationToken cancellationToken)
    {
        if (permissionIds.Count == 0)
        {
            return null;
        }

        var existingCount = await _permissionRepository.CountExistingIdsAsync(
            permissionIds,
            cancellationToken);

        if (existingCount != permissionIds.Count)
        {
            return "Geçersiz veya pasif izin ID değeri bulundu.";
        }

        return null;
    }

    private static int[] NormalizePermissionIds(int[]? permissionIds) =>
        (permissionIds ?? [])
            .Where(id => id > 0)
            .Distinct()
            .ToArray();

    private static RoleListItemDto ToListItemDto(RoleEntity entity) => new()
    {
        RoleId = entity.RoleId,
        RoleName = entity.RoleName,
        Description = entity.Description,
        IsActive = entity.IsActive
    };

    private static RoleDto ToDto(RoleEntity entity, IReadOnlyList<string> permissionCodes) => new()
    {
        RoleId = entity.RoleId,
        RoleName = entity.RoleName,
        Description = entity.Description,
        IsActive = entity.IsActive,
        PermissionCodes = permissionCodes
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(code => code, StringComparer.OrdinalIgnoreCase)
            .ToList()
    };
}

public sealed class UpdateRolePermissionsRequest
{
    public int RoleId { get; set; }

    public int[] PermissionIds { get; set; } = [];
}
