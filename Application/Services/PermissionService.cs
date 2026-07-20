using Application.DTOs;
using Core.Common;
using Infrastructure.Repositories;

namespace Application.Services;

public sealed class PermissionService
{
    private readonly PermissionRepository _permissionRepository;

    public PermissionService(PermissionRepository permissionRepository)
    {
        _permissionRepository = permissionRepository;
    }

    public async Task<ServiceResult<IReadOnlyList<PermissionDto>>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var permissions = await _permissionRepository.GetAllAsync(cancellationToken);
        return ServiceResult<IReadOnlyList<PermissionDto>>.Success(
            permissions.Select(ToDto).ToList());
    }

    private static PermissionDto ToDto(PermissionRecord entity) => new()
    {
        PermissionId = entity.PermissionId,
        PermissionCode = entity.PermissionCode,
        Description = entity.Description
    };
}
