using System.Security.Cryptography;
using Application.DTOs;
using Core.Authentication;
using Core.Authorization;
using Core.Common;
using Infrastructure.Entities;
using Infrastructure.Repositories;

namespace Application.Services;

public sealed class UserService
{
    private readonly UserRepository _userRepository;
    private readonly UserCompanyRoleRepository _userCompanyRoleRepository;
    private readonly BranchRepository _branchRepository;
    private readonly DepartmentRepository _departmentRepository;
    private readonly RoleRepository _roleRepository;
    private readonly IPasswordService _passwordService;
    private readonly ICompanyContext _companyContext;

    public UserService(
        UserRepository userRepository,
        UserCompanyRoleRepository userCompanyRoleRepository,
        BranchRepository branchRepository,
        DepartmentRepository departmentRepository,
        RoleRepository roleRepository,
        IPasswordService passwordService,
        ICompanyContext companyContext)
    {
        _userRepository = userRepository;
        _userCompanyRoleRepository = userCompanyRoleRepository;
        _branchRepository = branchRepository;
        _departmentRepository = departmentRepository;
        _roleRepository = roleRepository;
        _passwordService = passwordService;
        _companyContext = companyContext;
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

    public async Task<ServiceResult<IReadOnlyList<UserLookupDto>>> GetLookupAsync(
        CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetActiveLookupAsync(cancellationToken);
        return ServiceResult<IReadOnlyList<UserLookupDto>>.Success(
            users.Select(user => new UserLookupDto
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email
            }).ToList());
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

    public async Task<ServiceResult<int>> CreateAsync(
        CreateUserDto request,
        CancellationToken cancellationToken = default)
    {
        _companyContext.EnsureCanAccessCompany(request.CompanyId);

        var username = request.Username.Trim();
        if (await _userRepository.ExistsByUsernameAsync(username, cancellationToken: cancellationToken))
        {
            return ServiceResult<int>.Conflict("Bu kullanıcı adı zaten kullanılıyor.");
        }

        var role = await _roleRepository.GetByIdAsync(request.RoleId, cancellationToken);
        if (role is null || !role.IsActive)
        {
            return ServiceResult<int>.BadRequest("Geçersiz rol ID değeri.");
        }

        var organizationValidation = await ValidateOrganizationReferencesAsync(
            request.CompanyId,
            request.BranchId,
            request.DepartmentId,
            cancellationToken);
        if (organizationValidation is not null)
        {
            return organizationValidation;
        }

        var password = string.IsNullOrWhiteSpace(request.TemporaryPassword)
            ? GenerateSecurePassword()
            : request.TemporaryPassword.Trim();

        if (password.Length < 12)
        {
            return ServiceResult<int>.BadRequest(
                "Geçici şifre en az 12 karakter olmalıdır.");
        }

        var passwordHash = _passwordService.HashPassword(password);
        var azureObjectId = string.IsNullOrWhiteSpace(request.AzureObjectId)
            ? $"local:{username}"
            : request.AzureObjectId.Trim();

        var entity = new Users
        {
            AzureObjectId = azureObjectId,
            Username = username,
            PasswordHash = passwordHash,
            PasswordChangedAt = request.MustChangePassword ? null : DateTime.UtcNow,
            Email = request.Email.Trim(),
            FullName = request.FullName.Trim(),
            Title = request.Title,
            DepartmentId = request.DepartmentId,
            BranchId = request.BranchId,
            RoleId = request.RoleId,
            BirthDate = request.BirthDate,
            BirthMonth = request.BirthDate?.Month,
            BirthDay = request.BirthDate?.Day,
            HireDate = request.HireDate,
            Phone = request.Phone,
            PhotoUrl = request.PhotoUrl,
            IsActive = true
        };

        var userId = await _userRepository.CreateAsync(
            entity,
            cancellationToken);

        await _userCompanyRoleRepository.CreateAsync(
            userId,
            request.CompanyId,
            request.RoleId,
            cancellationToken);

        return ServiceResult<int>.Created(userId);
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

        var role = await _roleRepository.GetByIdAsync(request.RoleId, cancellationToken);
        if (role is null || !role.IsActive)
        {
            return ServiceResult.BadRequest("Geçersiz rol ID değeri.");
        }

        if (request.BranchId.HasValue || request.DepartmentId.HasValue)
        {
            var branch = request.BranchId.HasValue
                ? await _branchRepository.GetByIdAsync(request.BranchId.Value, cancellationToken)
                : null;

            var companyId = branch?.CompanyId;
            if (companyId.HasValue)
            {
                _companyContext.EnsureCanAccessCompany(companyId.Value);
            }

            var organizationValidation = await ValidateOrganizationReferencesAsync(
                companyId,
                request.BranchId,
                request.DepartmentId,
                cancellationToken);
            if (organizationValidation is not null)
            {
                return ServiceResult.BadRequest(organizationValidation.Message!);
            }
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

        if (!string.IsNullOrWhiteSpace(request.NewPassword))
        {
            var passwordHash = _passwordService.HashPassword(request.NewPassword);
            await _userRepository.UpdatePasswordAsync(
                request.UserId,
                passwordHash,
                DateTime.UtcNow,
                cancellationToken);
        }

        return ServiceResult.NoContent();
    }

    private async Task<ServiceResult<int>?> ValidateOrganizationReferencesAsync(
        int? companyId,
        int? branchId,
        int? departmentId,
        CancellationToken cancellationToken)
    {
        if (branchId.HasValue)
        {
            var branch = await _branchRepository.GetByIdAsync(branchId.Value, cancellationToken);
            if (branch is null)
            {
                return ServiceResult<int>.BadRequest("Geçersiz şube ID değeri.");
            }

            if (companyId.HasValue
                && branch.CompanyId.HasValue
                && branch.CompanyId.Value != companyId.Value)
            {
                return ServiceResult<int>.BadRequest(
                    "Seçilen şube belirtilen şirkete ait değil.");
            }
        }

        if (departmentId.HasValue)
        {
            var department = await _departmentRepository.GetByIdAsync(
                departmentId.Value,
                cancellationToken);
            if (department is null)
            {
                return ServiceResult<int>.BadRequest("Geçersiz departman ID değeri.");
            }

            if (branchId.HasValue
                && department.BranchId.HasValue
                && department.BranchId.Value != branchId.Value)
            {
                return ServiceResult<int>.BadRequest(
                    "Seçilen departman belirtilen şubeye ait değil.");
            }
        }

        return null;
    }

    private static string GenerateSecurePassword()
    {
        const string alphabet =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*";

        Span<char> chars = stackalloc char[16];
        for (var i = 0; i < chars.Length; i++)
        {
            var index = RandomNumberGenerator.GetInt32(alphabet.Length);
            chars[i] = alphabet[index];
        }

        return new string(chars);
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
