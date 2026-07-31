using Application.DTOs;
using Core.Authentication;
using Core.Authorization;
using Core.Common;
using Infrastructure.Repositories;

namespace Application.Services;

public sealed class ProfileService
{
    private readonly ICurrentUser _currentUser;
    private readonly UserRepository _userRepository;
    private readonly IPasswordService _passwordService;

    public ProfileService(
        ICurrentUser currentUser,
        UserRepository userRepository,
        IPasswordService passwordService)
    {
        _currentUser = currentUser;
        _userRepository = userRepository;
        _passwordService = passwordService;
    }

    public async Task<ServiceResult<CurrentUserProfileDto>> GetCurrentProfileAsync(
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.GetRequiredUserId();
        var enriched = await _userRepository.GetByIdEnrichedAsync(userId, cancellationToken);
        if (enriched is null)
        {
            return ServiceResult<CurrentUserProfileDto>.NotFound("Kullanıcı profili bulunamadı.");
        }

        var credential = await _userRepository.GetByIdAsync(userId, cancellationToken);

        var companies = _currentUser.CompanyMemberships
            .Select(membership => new CurrentUserCompanyResponse(
                membership.CompanyId,
                membership.CompanyName ?? string.Empty,
                membership.Roles))
            .ToArray();

        return ServiceResult<CurrentUserProfileDto>.Success(new CurrentUserProfileDto
        {
            UserId = enriched.UserId,
            Username = enriched.Username,
            Email = enriched.Email,
            FullName = enriched.FullName,
            Title = enriched.Title,
            Phone = enriched.Phone,
            PhotoUrl = enriched.PhotoUrl,
            IsActive = enriched.IsActive,
            RoleId = enriched.RoleId,
            RoleName = enriched.RoleName,
            CompanyId = enriched.CompanyId,
            CompanyName = enriched.CompanyName,
            BranchId = enriched.BranchId,
            BranchName = enriched.BranchName,
            DepartmentId = enriched.DepartmentId,
            DepartmentName = enriched.DepartmentName,
            BirthDate = enriched.BirthDate,
            HireDate = enriched.HireDate,
            LastLoginAt = enriched.LastLoginAt,
            PasswordChangedAt = credential?.PasswordChangedAt,
            CreatedAt = enriched.CreatedAt,
            UpdatedAt = enriched.UpdatedAt,
            Roles = _currentUser.Roles,
            Permissions = _currentUser.Permissions,
            Companies = companies
        });
    }

    public async Task<ServiceResult> ChangeCurrentPasswordAsync(
        ChangeMyPasswordDto request,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.GetRequiredUserId();

        var currentPassword = request.CurrentPassword ?? string.Empty;
        var newPassword = request.NewPassword ?? string.Empty;
        var confirmPassword = request.ConfirmPassword ?? string.Empty;

        if (string.IsNullOrWhiteSpace(currentPassword)
            || string.IsNullOrWhiteSpace(newPassword)
            || string.IsNullOrWhiteSpace(confirmPassword))
        {
            return ServiceResult.BadRequest("Şifre alanları zorunludur.");
        }

        if (!string.Equals(newPassword, confirmPassword, StringComparison.Ordinal))
        {
            return ServiceResult.BadRequest("Yeni şifre ile tekrarı eşleşmiyor.");
        }

        if (string.Equals(currentPassword, newPassword, StringComparison.Ordinal))
        {
            return ServiceResult.BadRequest("Yeni şifre mevcut şifre ile aynı olamaz.");
        }

        if (newPassword.Length < 12 || newPassword.Length > 128)
        {
            return ServiceResult.BadRequest("Yeni şifre 12–128 karakter arasında olmalıdır.");
        }

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return ServiceResult.NotFound("Kullanıcı bulunamadı.");
        }

        if (string.IsNullOrWhiteSpace(user.PasswordHash))
        {
            return ServiceResult.BadRequest(
                "Bu hesap için yerel şifre tanımlı değil. Şifre değiştirilemez.");
        }

        if (!_passwordService.VerifyPassword(currentPassword, user.PasswordHash))
        {
            return ServiceResult.BadRequest("Mevcut şifre hatalı.");
        }

        var hash = _passwordService.HashPassword(newPassword);
        var rows = await _userRepository.UpdatePasswordAsync(
            userId,
            hash,
            DateTime.UtcNow,
            cancellationToken);

        if (rows == 0)
        {
            return ServiceResult.Conflict("Şifre güncellenemedi.");
        }

        return ServiceResult.NoContent();
    }
}
