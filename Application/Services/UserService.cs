using Application.DTOs;
using Core.Common;
using Infrastructure.Entities;
using Infrastructure.Repositories;

namespace Application.Services;

public sealed class UserService
{
    private readonly UserRepository _userRepository;

    public UserService(
        UserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<ServiceResult<IReadOnlyList<UserDto>>>
        GetAllAsync(
            CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetAllAsync(
            cancellationToken);

        IReadOnlyList<UserDto> response = users
            .Select(ToDto)
            .ToList();

        return ServiceResult<IReadOnlyList<UserDto>>
            .Success(response);
    }

    public async Task<ServiceResult<IReadOnlyList<UserDto>>>
        GetActiveAsync(
            CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetActiveAsync(
            cancellationToken);

        IReadOnlyList<UserDto> response = users
            .Select(ToDto)
            .ToList();

        return ServiceResult<IReadOnlyList<UserDto>>
            .Success(response);
    }

    public async Task<ServiceResult<UserDto>> GetByIdAsync(
        IdRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _userRepository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (entity is null)
        {
            return ServiceResult<UserDto>.NotFound(
                $"ID değeri {request.Id} olan kullanıcı bulunamadı.");
        }

        return ServiceResult<UserDto>.Success(
            ToDto(entity));
    }

    public async Task<ServiceResult<IReadOnlyList<UserDto>>>
        SearchAsync(
            string searchText,
            CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return ServiceResult<IReadOnlyList<UserDto>>
                .BadRequest("Arama metni boş olamaz.");
        }

        var users = await _userRepository.SearchAsync(
            searchText.Trim(),
            cancellationToken);

        IReadOnlyList<UserDto> response = users
            .Select(ToDto)
            .ToList();

        return ServiceResult<IReadOnlyList<UserDto>>
            .Success(response);
    }

    public async Task<ServiceResult> UpdateAsync(
        UpdateUserDto request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _userRepository.GetByIdAsync(
            request.UserId,
            cancellationToken);

        if (entity is null)
        {
            return ServiceResult.NotFound(
                $"ID değeri {request.UserId} olan kullanıcı bulunamadı.");
        }

        entity.FullName = request.FullName;
        entity.Title = request.Title;
        entity.DepartmentId = request.DepartmentId;
        entity.BranchId = request.BranchId;
        entity.RoleId = request.RoleId;
        entity.BirthDate = request.BirthDate;
        entity.BirthMonth = request.BirthDate?.Month;
        entity.BirthDay = request.BirthDate?.Day;
        entity.HireDate = request.HireDate;
        entity.Phone = request.Phone;
        entity.PhotoUrl = request.PhotoUrl;
        entity.IsActive = request.IsActive;

        var rowsAffected = await _userRepository.UpdateAsync(
            entity,
            cancellationToken);

        if (rowsAffected == 0)
        {
            return ServiceResult.Conflict(
                "Kullanıcı güncellenemedi.");
        }

        return ServiceResult.NoContent();
    }

    private static UserDto ToDto(
        Users entity)
    {
        return new UserDto
        {
            UserId = entity.UserId,
            AzureObjectId = entity.AzureObjectId,
            Email = entity.Email,
            FullName = entity.FullName,
            Title = entity.Title,
            DepartmentId = entity.DepartmentId,
            BranchId = entity.BranchId,
            RoleId = entity.RoleId,
            BirthDate = entity.BirthDate,
            HireDate = entity.HireDate,
            Phone = entity.Phone,
            PhotoUrl = entity.PhotoUrl,
            IsActive = entity.IsActive,
            LastLoginAt = entity.LastLoginAt,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
}