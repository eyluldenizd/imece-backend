using Application.DTOs;
using Application.Exceptions;
using Core.Authorization;
using Core.Common;
using Infrastructure.Repositories;

namespace Application.Services;

/// <summary>
/// Kullanıcıya çoklu rol ve şirket erişimi atama. Rol ⊥ şirket erişimi.
/// PUT işlemleri full replacement + atomic dual-write.
/// </summary>
public sealed class UserAccessService
{
    private readonly UserRepository _userRepository;
    private readonly RoleRepository _roleRepository;
    private readonly CompanyRepository _companyRepository;
    private readonly UserRoleRepository _userRoleRepository;
    private readonly UserCompanyAccessRepository _userCompanyAccessRepository;
    private readonly UserAccessWriteRepository _userAccessWriteRepository;
    private readonly ICompanyAuthorizationService _companyAuthorization;
    private readonly ICurrentUser _currentUser;

    public UserAccessService(
        UserRepository userRepository,
        RoleRepository roleRepository,
        CompanyRepository companyRepository,
        UserRoleRepository userRoleRepository,
        UserCompanyAccessRepository userCompanyAccessRepository,
        UserAccessWriteRepository userAccessWriteRepository,
        ICompanyAuthorizationService companyAuthorization,
        ICurrentUser currentUser)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _companyRepository = companyRepository;
        _userRoleRepository = userRoleRepository;
        _userCompanyAccessRepository = userCompanyAccessRepository;
        _userAccessWriteRepository = userAccessWriteRepository;
        _companyAuthorization = companyAuthorization;
        _currentUser = currentUser;
    }

    public async Task<ServiceResult<IReadOnlyList<UserRoleAssignmentDto>>> GetUserRolesAsync(
        IdRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync((int)request.Id, cancellationToken);
        if (user is null)
        {
            return ServiceResult<IReadOnlyList<UserRoleAssignmentDto>>.NotFound(
                $"ID değeri {request.Id} olan kullanıcı bulunamadı.");
        }

        var roles = await _userRoleRepository.GetByUserIdAsync(user.UserId, cancellationToken);
        return ServiceResult<IReadOnlyList<UserRoleAssignmentDto>>.Success(
            roles
                .OrderBy(r => r.RoleName, StringComparer.OrdinalIgnoreCase)
                .Select(r => new UserRoleAssignmentDto
                {
                    RoleId = r.RoleId,
                    RoleName = r.RoleName,
                    Description = r.Description,
                    IsSystem = SystemRoleCatalog.IsSystemRole(r.RoleName)
                })
                .ToList());
    }

    public async Task<ServiceResult> UpdateUserRolesAsync(
        UpdateUserRolesRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return ServiceResult.NotFound(
                $"ID değeri {request.UserId} olan kullanıcı bulunamadı.");
        }

        var roleIds = (request.RoleIds ?? [])
            .Where(id => id > 0)
            .Distinct()
            .ToArray();

        if (roleIds.Length == 0)
        {
            return ServiceResult.BadRequest("En az bir rol atanmalıdır.");
        }

        var assignsGlobalAdmin = false;

        foreach (var roleId in roleIds)
        {
            var role = await _roleRepository.GetByIdAsync(roleId, cancellationToken);
            if (role is null || !role.IsActive)
            {
                return ServiceResult.BadRequest($"Geçersiz veya pasif rol ID: {roleId}.");
            }

            if (string.Equals(role.RoleName, Roles.GlobalAdmin, StringComparison.OrdinalIgnoreCase))
            {
                assignsGlobalAdmin = true;
                if (!_companyAuthorization.CanAccessAllCompanies)
                {
                    throw new ForbiddenException(
                        "Global organizasyon erişimi olan kullanıcılar global admin rolü atayabilir.");
                }
            }
        }

        var currentRoles = await _userRoleRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        var currentlyGlobalAdmin = currentRoles.Any(r =>
            string.Equals(r.RoleName, Roles.GlobalAdmin, StringComparison.OrdinalIgnoreCase));

        if (currentlyGlobalAdmin && !assignsGlobalAdmin)
        {
            var lockoutError = await EnsureNotLastGlobalAdminAsync(cancellationToken);
            if (lockoutError is not null)
            {
                return ServiceResult.Conflict(lockoutError);
            }
        }

        var companyIds = await _userCompanyAccessRepository.GetCompanyIdsForUserAsync(
            request.UserId,
            cancellationToken);

        await _userAccessWriteRepository.ReplaceRolesAsync(
            request.UserId,
            roleIds,
            companyIds,
            cancellationToken);

        return ServiceResult.NoContent();
    }

    /// <summary>
    /// Hedef kullanıcının doğrudan user_company_access atamaları.
    /// Global scope kullanıcıda companies boş olabilir; scope alanı yine döner.
    /// </summary>
    public async Task<ServiceResult<UserCompanyAssignmentsResponse>> GetUserCompaniesAsync(
        IdRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync((int)request.Id, cancellationToken);
        if (user is null)
        {
            return ServiceResult<UserCompanyAssignmentsResponse>.NotFound(
                $"ID değeri {request.Id} olan kullanıcı bulunamadı.");
        }

        var hasGlobal = await ResolveHasGlobalOrganizationAccessAsync(
            user.UserId,
            cancellationToken);

        var assignments = await _userCompanyAccessRepository.GetByUserIdAsync(
            user.UserId,
            cancellationToken);

        var companies = assignments
            .OrderBy(c => c.CompanyName, StringComparer.OrdinalIgnoreCase)
            .Select(c => new UserCompanyAccessDto
            {
                CompanyId = c.CompanyId,
                CompanyCode = c.CompanyCode,
                CompanyName = c.CompanyName,
                IsActive = c.CompanyIsActive && c.IsActive
            })
            .ToList();

        return ServiceResult<UserCompanyAssignmentsResponse>.Success(
            new UserCompanyAssignmentsResponse
            {
                UserId = user.UserId,
                OrganizationScope = OrganizationScopeCodes.ToCode(
                    OrganizationScopeCodes.FromHasGlobalAccess(hasGlobal)),
                CanAccessAllCompanies = hasGlobal,
                // Global kullanıcıda assignments boş olabilir; kayıt varsa admin görünürlüğü için döner.
                Companies = companies
            });
    }

    /// <summary>
    /// Full replacement of user_company_access (+ legacy UCR dual-write).
    /// </summary>
    public async Task<ServiceResult> UpdateUserCompaniesAsync(
        UpdateUserCompaniesRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return ServiceResult.NotFound(
                $"ID değeri {request.UserId} olan kullanıcı bulunamadı.");
        }

        var companyIds = (request.CompanyIds ?? [])
            .Where(id => id > 0)
            .Distinct()
            .ToArray();

        foreach (var companyId in companyIds)
        {
            if (!_companyAuthorization.CanAccessCompany(companyId))
            {
                throw new ForbiddenException(
                    $"Şirket {companyId} için erişim yetkiniz yok; atama yapılamaz.");
            }

            var company = await _companyRepository.GetByIdAsync(companyId, cancellationToken);
            if (company is null)
            {
                return ServiceResult.NotFound($"Şirket bulunamadı: {companyId}.");
            }

            if (!company.IsActive)
            {
                return ServiceResult.BadRequest(
                    $"Pasif şirkete erişim atanamaz: {companyId} ({company.CompanyName}).");
            }
        }

        var roleIds = (await _userRoleRepository.GetByUserIdAsync(request.UserId, cancellationToken))
            .Select(r => r.RoleId)
            .ToArray();

        if (roleIds.Length == 0 && user.RoleId > 0)
        {
            roleIds = [user.RoleId];
        }

        await _userAccessWriteRepository.ReplaceCompaniesAsync(
            request.UserId,
            companyIds,
            roleIds,
            cancellationToken);

        return ServiceResult.NoContent();
    }

    /// <summary>
    /// Current user için erişilebilir şirket listesi (lookup).
    /// Global: tüm aktif şirketler. Assigned: user_company_access.
    /// </summary>
    public async Task<ServiceResult<AccessibleCompaniesResponse>> GetAccessibleCompaniesAsync(
        CancellationToken cancellationToken = default)
    {
        var canAccessAll = _companyAuthorization.CanAccessAllCompanies;
        var organizationScope = OrganizationScopeCodes.ToCode(
            OrganizationScopeCodes.FromHasGlobalAccess(canAccessAll));

        if (canAccessAll)
        {
            var allActive = await _companyRepository.GetActiveAsync(cancellationToken);
            return ServiceResult<AccessibleCompaniesResponse>.Success(
                new AccessibleCompaniesResponse
                {
                    OrganizationScope = organizationScope,
                    CanAccessAllCompanies = true,
                    Companies = allActive
                        .OrderBy(c => c.CompanyName, StringComparer.OrdinalIgnoreCase)
                        .Select(c => new AccessibleCompanyDto
                        {
                            CompanyId = c.CompanyId,
                            CompanyCode = c.CompanyCode,
                            CompanyName = c.CompanyName,
                            Roles = []
                        })
                        .ToList()
                });
        }

        var userId = _currentUser.UserId;
        if (userId is null)
        {
            return ServiceResult<AccessibleCompaniesResponse>.Success(
                new AccessibleCompaniesResponse
                {
                    OrganizationScope = organizationScope,
                    CanAccessAllCompanies = false,
                    Companies = []
                });
        }

        var assignments = await _userCompanyAccessRepository.GetByUserIdAsync(
            userId.Value,
            cancellationToken);

        var rolesByCompany = _currentUser.CompanyMemberships
            .ToDictionary(
                m => m.CompanyId,
                m => m.Roles,
                EqualityComparer<int>.Default);

        var companies = assignments
            .Where(a => a.CompanyIsActive)
            .OrderBy(a => a.CompanyName, StringComparer.OrdinalIgnoreCase)
            .Select(a => new AccessibleCompanyDto
            {
                CompanyId = a.CompanyId,
                CompanyCode = a.CompanyCode,
                CompanyName = a.CompanyName,
                Roles = rolesByCompany.TryGetValue(a.CompanyId, out var roles)
                    ? roles
                    : []
            })
            .ToList();

        // Fallback: memberships if assignments empty but context has companies (legacy).
        if (companies.Count == 0 && _currentUser.CompanyMemberships.Count > 0)
        {
            companies = _currentUser.CompanyMemberships
                .OrderBy(m => m.CompanyName, StringComparer.OrdinalIgnoreCase)
                .Select(m => new AccessibleCompanyDto
                {
                    CompanyId = m.CompanyId,
                    CompanyCode = null,
                    CompanyName = m.CompanyName ?? string.Empty,
                    Roles = m.Roles
                })
                .ToList();
        }

        return ServiceResult<AccessibleCompaniesResponse>.Success(
            new AccessibleCompaniesResponse
            {
                OrganizationScope = organizationScope,
                CanAccessAllCompanies = false,
                Companies = companies
            });
    }

    public bool CanAccessCompany(int companyId) =>
        _companyAuthorization.CanAccessCompany(companyId);

    private async Task<bool> ResolveHasGlobalOrganizationAccessAsync(
        int userId,
        CancellationToken cancellationToken)
    {
        var roles = await _userRoleRepository.GetByUserIdAsync(userId, cancellationToken);
        return roles.Any(r =>
            string.Equals(r.RoleName, Roles.GlobalAdmin, StringComparison.OrdinalIgnoreCase));
    }

    private async Task<string?> EnsureNotLastGlobalAdminAsync(CancellationToken cancellationToken)
    {
        var globalRole = (await _roleRepository.GetAllAsync(cancellationToken))
            .FirstOrDefault(r =>
                string.Equals(r.RoleName, Roles.GlobalAdmin, StringComparison.OrdinalIgnoreCase));

        if (globalRole is null)
        {
            return null;
        }

        var count = await _userRoleRepository.CountUsersWithRoleAsync(
            globalRole.RoleId,
            cancellationToken);

        if (count <= 1)
        {
            return "Son global admin kullanıcısının global admin rolü kaldırılamaz.";
        }

        return null;
    }
}
