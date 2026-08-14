using System.Security.Cryptography;
using Application.Common.CompanyScope;
using Application.Common.ListQuery;
using Application.DTOs;
using Application.Exceptions;
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
    private readonly UserRoleRepository _userRoleRepository;
    private readonly UserCompanyAccessRepository _userCompanyAccessRepository;
    private readonly BranchRepository _branchRepository;
    private readonly DepartmentRepository _departmentRepository;
    private readonly RoleRepository _roleRepository;
    private readonly IPasswordService _passwordService;
    private readonly ICompanyContext _companyContext;
    private readonly ICurrentUser _currentUser;

    public UserService(
        UserRepository userRepository,
        UserCompanyRoleRepository userCompanyRoleRepository,
        UserRoleRepository userRoleRepository,
        UserCompanyAccessRepository userCompanyAccessRepository,
        BranchRepository branchRepository,
        DepartmentRepository departmentRepository,
        RoleRepository roleRepository,
        IPasswordService passwordService,
        ICompanyContext companyContext,
        ICurrentUser currentUser)
    {
        _userRepository = userRepository;
        _userCompanyRoleRepository = userCompanyRoleRepository;
        _userRoleRepository = userRoleRepository;
        _userCompanyAccessRepository = userCompanyAccessRepository;
        _branchRepository = branchRepository;
        _departmentRepository = departmentRepository;
        _roleRepository = roleRepository;
        _passwordService = passwordService;
        _companyContext = companyContext;
        _currentUser = currentUser;
    }

    public async Task<ServiceResult<IReadOnlyList<UserDto>>>
        GetAllAsync(
            ContentListQueryDto? query = null,
            CancellationToken cancellationToken = default)
    {
        var filter = CompanyScopeRules.ResolveListCompanyFilter(_companyContext, _currentUser);
        var users = await _userRepository.GetAllEnrichedAsync(filter, cancellationToken);

        return ServiceResult<IReadOnlyList<UserDto>>.Success(
            AdminListQueryProfiles.ApplyToUsers(users.Select(ToDto), query));
    }

    /// <summary>
    /// Authorized, company-scoped user list with search/filter/sort + server-side paging.
    /// Caller identity comes from <see cref="ICurrentUser"/> / <see cref="ICompanyContext"/>
    /// (token middleware) — never from a client-supplied user id.
    /// Scope: global_admin sees all orgs; otherwise membership via CompanyScopeSql.UserMembershipFilter.
    /// </summary>
    public async Task<ServiceResult<PagedResultDto<UserDto>>>
        GetAuthorizedPagedAsync(
            ContentListQueryDto? query = null,
            CancellationToken cancellationToken = default)
    {
        // Company/org scope is applied in SQL; remaining filters/sort run as deferred LINQ
        // on that scoped set, then Skip/Take materializes only the requested page.
        var filter = CompanyScopeRules.ResolveListCompanyFilter(_companyContext, _currentUser);
        var users = await _userRepository.GetAllEnrichedAsync(filter, cancellationToken);

        var filtered = AdminListQueryProfiles.ApplyToUsers(users.Select(ToDto), query);
        var page = ContentListQueryApplier.ApplyPaging(filtered, query);

        return ServiceResult<PagedResultDto<UserDto>>.Success(page);
    }

    public async Task<ServiceResult<IReadOnlyList<UserDto>>>
        GetActiveAsync(
            CancellationToken cancellationToken = default)
    {
        var filter = CompanyScopeRules.ResolveListCompanyFilter(_companyContext, _currentUser);
        var users = await _userRepository.GetActiveEnrichedAsync(filter, cancellationToken);

        return ServiceResult<IReadOnlyList<UserDto>>.Success(
            users.Select(ToDto).ToList());
    }

    public async Task<ServiceResult<IReadOnlyList<UserLookupDto>>> GetLookupAsync(
        CancellationToken cancellationToken = default)
    {
        var filter = CompanyScopeRules.ResolveListCompanyFilter(_companyContext, _currentUser);
        var users = await _userRepository.GetActiveLookupAsync(filter, cancellationToken);

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
        var record = await _userRepository.GetByIdEnrichedAsync(
            (int)request.Id,
            cancellationToken);

        if (record is null)
        {
            return ServiceResult<UserDto>.NotFound(
                $"ID değeri {request.Id} olan kullanıcı bulunamadı.");
        }

        await EnsureUserAccessAsync(record.CompanyId, record.BranchId, cancellationToken);

        return ServiceResult<UserDto>.Success(ToDto(record));
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

        var filter = CompanyScopeRules.ResolveListCompanyFilter(_companyContext, _currentUser);
        var users = await _userRepository.SearchEnrichedAsync(
            searchText.Trim(),
            filter,
            cancellationToken);

        return ServiceResult<IReadOnlyList<UserDto>>.Success(
            users.Select(ToDto).ToList());
    }

    public async Task<ServiceResult<int>> CreateAsync(
        CreateUserDto request,
        CancellationToken cancellationToken = default)
    {
        _companyContext.EnsureCanAccessCompany(request.CompanyId);

        var email = request.Email.Trim();
        var usernameResult = await ResolveUsernameAsync(email, request.Username, cancellationToken);
        if (usernameResult.Error is not null)
        {
            return usernameResult.Error;
        }

        var username = usernameResult.Username!;

        var role = await _roleRepository.GetByIdAsync(request.RoleId, cancellationToken);
        if (role is null || !role.IsActive)
        {
            return ServiceResult<int>.BadRequest("Geçersiz rol ID değeri.");
        }

        var roleValidation = ValidateRoleAssignment(role);
        if (roleValidation is not null)
        {
            return roleValidation;
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

        await _userRoleRepository.InsertAsync(userId, request.RoleId, cancellationToken);
        await _userCompanyAccessRepository.InsertAsync(
            userId,
            request.CompanyId,
            cancellationToken);

        return ServiceResult<int>.Created(userId);
    }

    public async Task<ServiceResult> UpdateAsync(
        UpdateUserDto request,
        CancellationToken cancellationToken = default)
    {
        var existing = await _userRepository.GetByIdEnrichedAsync(
            request.UserId,
            cancellationToken);

        if (existing is null)
        {
            return ServiceResult.NotFound(
                $"ID değeri {request.UserId} olan kullanıcı bulunamadı.");
        }

        await EnsureUserAccessAsync(existing.CompanyId, existing.BranchId, cancellationToken);

        var role = await _roleRepository.GetByIdAsync(request.RoleId, cancellationToken);
        if (role is null || !role.IsActive)
        {
            return ServiceResult.BadRequest("Geçersiz rol ID değeri.");
        }

        var roleValidation = ValidateRoleAssignment(role);
        if (roleValidation is not null)
        {
            return ServiceResult.BadRequest(roleValidation.Message!);
        }

        var targetCompanyId = await ResolveTargetCompanyIdAsync(
            request.CompanyId,
            request.BranchId,
            existing,
            cancellationToken);

        if (targetCompanyId.HasValue)
        {
            _companyContext.EnsureCanAccessCompany(targetCompanyId.Value);
        }
        else if (!_companyContext.IsGlobalAdmin)
        {
            throw new ForbiddenException("Bu kullanıcıya erişim yetkiniz bulunmuyor.");
        }

        if (request.BranchId.HasValue || request.DepartmentId.HasValue)
        {
            var organizationValidation = await ValidateOrganizationReferencesAsync(
                targetCompanyId,
                request.BranchId,
                request.DepartmentId,
                cancellationToken);
            if (organizationValidation is not null)
            {
                return ServiceResult.BadRequest(organizationValidation.Message!);
            }
        }

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

    private async Task<int?> ResolveTargetCompanyIdAsync(
        int? requestedCompanyId,
        int? branchId,
        UserEnrichedRecord existing,
        CancellationToken cancellationToken)
    {
        if (requestedCompanyId.HasValue && requestedCompanyId.Value > 0)
        {
            return requestedCompanyId.Value;
        }

        if (branchId.HasValue)
        {
            var branch = await _branchRepository.GetByIdAsync(branchId.Value, cancellationToken);
            return branch?.CompanyId;
        }

        return existing.CompanyId;
    }

    private async Task EnsureUserAccessAsync(
        int? companyId,
        int? branchId,
        CancellationToken cancellationToken)
    {
        if (_companyContext.IsGlobalAdmin)
        {
            return;
        }

        if (companyId.HasValue && _companyContext.CanAccessCompany(companyId.Value))
        {
            return;
        }

        if (branchId.HasValue)
        {
            var branch = await _branchRepository.GetByIdAsync(branchId.Value, cancellationToken);
            if (branch?.CompanyId is int branchCompanyId
                && _companyContext.CanAccessCompany(branchCompanyId))
            {
                return;
            }
        }

        throw new ForbiddenException("Bu kullanıcıya erişim yetkiniz bulunmuyor.");
    }

    private ServiceResult<int>? ValidateRoleAssignment(Infrastructure.Entities.Roles role)
    {
        if (_companyContext.IsGlobalAdmin)
        {
            return null;
        }

        if (string.Equals(role.RoleName, Core.Authorization.Roles.GlobalAdmin, StringComparison.OrdinalIgnoreCase))
        {
            return ServiceResult<int>.BadRequest(
                "Platform yöneticisi rolü yalnızca sistem yöneticisi tarafından atanabilir.");
        }

        return null;
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

            if (!_companyContext.IsGlobalAdmin
                && branch.CompanyId.HasValue)
            {
                _companyContext.EnsureCanAccessCompany(branch.CompanyId.Value);
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

    private async Task<(string? Username, ServiceResult<int>? Error)> ResolveUsernameAsync(
        string email,
        string? requestedUsername,
        CancellationToken cancellationToken)
    {
        var baseUsername = !string.IsNullOrWhiteSpace(requestedUsername)
            ? requestedUsername.Trim()
            : DeriveUsernameFromEmail(email);

        if (string.IsNullOrWhiteSpace(baseUsername))
        {
            return (null, ServiceResult<int>.BadRequest("Geçerli bir e-posta adresinden kullanıcı adı üretilemedi."));
        }

        if (baseUsername.Length > 128)
        {
            baseUsername = baseUsername[..128];
        }

        var candidate = baseUsername;
        for (var suffix = 1; suffix <= 99; suffix++)
        {
            if (!await _userRepository.ExistsByUsernameAsync(candidate, cancellationToken: cancellationToken))
            {
                return (candidate, null);
            }

            var suffixText = $"-{suffix}";
            var maxBaseLength = 128 - suffixText.Length;
            var trimmedBase = baseUsername.Length > maxBaseLength
                ? baseUsername[..maxBaseLength]
                : baseUsername;
            candidate = $"{trimmedBase}{suffixText}";
        }

        return (null, ServiceResult<int>.Conflict("Bu e-posta için benzersiz bir kullanıcı adı üretilemedi."));
    }

    private static string DeriveUsernameFromEmail(string email)
    {
        var atIndex = email.IndexOf('@');
        if (atIndex <= 0)
        {
            return string.Empty;
        }

        var localPart = email[..atIndex].Trim().ToLowerInvariant();
        Span<char> buffer = stackalloc char[localPart.Length];
        var length = 0;

        foreach (var character in localPart)
        {
            if (char.IsAsciiLetterOrDigit(character) || character is '.' or '_' or '-')
            {
                buffer[length++] = character;
            }
            else if (character is ' ')
            {
                buffer[length++] = '.';
            }
        }

        return length == 0 ? string.Empty : new string(buffer[..length]);
    }

    private static UserDto ToDto(UserEnrichedRecord record) => new()
    {
        UserId = record.UserId,
        AzureObjectId = record.AzureObjectId,
        Username = record.Username,
        Email = record.Email,
        FullName = record.FullName,
        Title = record.Title,
        CompanyId = record.CompanyId,
        CompanyName = record.CompanyName,
        DepartmentId = record.DepartmentId,
        DepartmentName = record.DepartmentName,
        BranchId = record.BranchId,
        BranchName = record.BranchName,
        RoleId = record.RoleId,
        RoleName = record.RoleName,
        BirthDate = record.BirthDate,
        HireDate = record.HireDate,
        Phone = record.Phone,
        PhotoUrl = record.PhotoUrl,
        IsActive = record.IsActive,
        LastLoginAt = record.LastLoginAt,
        CreatedAt = record.CreatedAt,
        UpdatedAt = record.UpdatedAt
    };
}
