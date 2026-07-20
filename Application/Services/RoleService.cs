using Application.DTOs;
using Core.Common;
using Infrastructure.Entities;
using Infrastructure.Repositories;

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
        CancellationToken cancellationToken = default)
    {
        var roles = await _roleRepository.GetAllAsync(cancellationToken);
        return ServiceResult<IReadOnlyList<RoleListItemDto>>.Success(
            roles.Select(ToListItemDto).ToList());
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
        var roleName = request.RoleName.Trim();
        if (await _roleRepository.ExistsByNameAsync(roleName, cancellationToken: cancellationToken))
        {
            return ServiceResult<int>.Conflict("Bu rol adı zaten kullanılıyor.");
        }

        var entity = new Roles
        {
            RoleName = roleName,
            Description = request.Description?.Trim(),
            IsActive = request.IsActive
        };

        var roleId = await _roleRepository.CreateAsync(entity, cancellationToken);
        return ServiceResult<int>.Created(roleId);
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

        var roleName = request.RoleName.Trim();
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

        var rows = await _roleRepository.UpdateAsync(existing, cancellationToken);
        if (rows == 0)
        {
            return ServiceResult.Conflict("Rol güncellenemedi.");
        }

        return ServiceResult.NoContent();
    }

    public async Task<ServiceResult> DeleteAsync(
        IdRequest request,
        CancellationToken cancellationToken = default)
    {
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

        var permissionIds = request.PermissionIds ?? [];
        if (permissionIds.Length > 0)
        {
            var existingCount = await _permissionRepository.CountExistingIdsAsync(
                permissionIds,
                cancellationToken);

            if (existingCount != permissionIds.Distinct().Count())
            {
                return ServiceResult.BadRequest("Geçersiz izin ID değeri bulundu.");
            }
        }

        await _roleRepository.ReplacePermissionsAsync(
            request.RoleId,
            permissionIds,
            cancellationToken);

        return ServiceResult.NoContent();
    }

    private static RoleListItemDto ToListItemDto(Roles entity) => new()
    {
        RoleId = entity.RoleId,
        RoleName = entity.RoleName,
        Description = entity.Description,
        IsActive = entity.IsActive
    };

    private static RoleDto ToDto(Roles entity, IReadOnlyList<string> permissionCodes) => new()
    {
        RoleId = entity.RoleId,
        RoleName = entity.RoleName,
        Description = entity.Description,
        IsActive = entity.IsActive,
        PermissionCodes = permissionCodes
    };
}

public sealed class UpdateRolePermissionsRequest
{
    public int RoleId { get; set; }

    public int[] PermissionIds { get; set; } = [];
}
