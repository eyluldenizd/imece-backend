using Application.DTOs;
using Application.Exceptions;
using Application.Services;
using Core.Authentication;
using Core.Authorization;
using Core.Directory;
using Infrastructure.Repositories;

namespace Application.Services;

/// <summary>
/// LocalJwt username/password login. Dev profile login root causes addressed:
/// (1) login endpoint, (2) FE GuestGuard via real JWT, (3) SqlDirectory ExternalId
/// alignment via azure_object_id, (4) permissions from role_permissions not hardcoded.
/// </summary>
public sealed class AuthenticationService : IAuthenticationService
{
    private const string InvalidCredentialsMessage = "Kullanıcı adı veya şifre hatalı.";
    private const string AdminPanelDeniedMessage = "Bu kullanıcı admin paneline erişemez.";

    private readonly UserCredentialRepository _credentialRepository;
    private readonly IPasswordService _passwordService;
    private readonly IDirectoryUserService _directoryUserService;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthenticationService(
        UserCredentialRepository credentialRepository,
        IPasswordService passwordService,
        IDirectoryUserService directoryUserService,
        IJwtTokenService jwtTokenService)
    {
        _credentialRepository = credentialRepository;
        _passwordService = passwordService;
        _directoryUserService = directoryUserService;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<LoginResponseDto> LoginAsync(
        LoginRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Username)
            || string.IsNullOrWhiteSpace(request.Password))
        {
            throw new UnauthorizedAppException(InvalidCredentialsMessage);
        }

        var normalizedUsername = request.Username.Trim().ToLowerInvariant();

        var credential = await _credentialRepository.FindByUsernameAsync(
            normalizedUsername,
            cancellationToken);

        if (credential is null
            || string.IsNullOrWhiteSpace(credential.PasswordHash)
            || !_passwordService.VerifyPassword(request.Password, credential.PasswordHash))
        {
            if (credential is not null)
            {
                await _credentialRepository.UpdateLoginFailureAsync(
                    credential.UserId,
                    cancellationToken);
            }

            throw new UnauthorizedAppException(InvalidCredentialsMessage);
        }

        if (credential.LockoutEnd is DateTime lockoutEnd && lockoutEnd > DateTime.UtcNow)
        {
            throw new UnauthorizedAppException(InvalidCredentialsMessage);
        }

        if (!credential.IsActive)
        {
            throw new UnauthorizedAppException(InvalidCredentialsMessage);
        }

        var identity = new ExternalIdentity
        {
            IdentityProvider = ImeceIdentityProviders.Local,
            ExternalId = credential.AzureObjectId,
            Username = credential.Username
        };

        var applicationUser = await _directoryUserService.FindByExternalIdentityAsync(
            identity,
            cancellationToken);

        if (applicationUser is null || !applicationUser.IsRegistered)
        {
            throw new UnauthorizedAppException(InvalidCredentialsMessage);
        }

        if (!HasPermission(applicationUser, Permissions.AdminPanelAccess))
        {
            throw new ForbiddenException(AdminPanelDeniedMessage);
        }

        var (accessToken, expiresAt) = _jwtTokenService.CreateAccessToken(applicationUser);

        await _credentialRepository.UpdateLoginSuccessAsync(
            credential.UserId,
            cancellationToken);

        var userResponse = MapToCurrentUserResponse(applicationUser);

        return new LoginResponseDto(
            accessToken,
            expiresAt,
            "Bearer",
            userResponse);
    }

    internal static CurrentUserResponse MapToCurrentUserResponse(ApplicationUser user)
    {
        var companies = user.CompanyMemberships
            .Select(membership => new CurrentUserCompanyResponse(
                membership.CompanyId,
                membership.CompanyName ?? string.Empty,
                membership.Roles))
            .ToArray();

        return new CurrentUserResponse(
            UserId: user.UserId
                ?? throw new InvalidOperationException("Registered user must have UserId."),
            Username: user.Identity.Username ?? string.Empty,
            Email: user.Identity.Email ?? string.Empty,
            DisplayName: user.Identity.DisplayName ?? string.Empty,
            ActiveCompanyId: user.CompanyId,
            ActiveCompanyName: user.CompanyName,
            Roles: user.Roles,
            Permissions: user.Permissions,
            Companies: companies,
            HasAdminPanelAccess: HasPermission(user, Permissions.AdminPanelAccess));
    }

    private static bool HasPermission(ApplicationUser user, string permission) =>
        user.Permissions.Contains(permission, StringComparer.OrdinalIgnoreCase);
}
