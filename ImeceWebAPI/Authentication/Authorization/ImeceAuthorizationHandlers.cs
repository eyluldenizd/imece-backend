using Core.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace ImeceWebAPI.Authentication.Authorization;

/// <summary>
/// Tüm policy handler'ları kararı yalnızca <see cref="ICurrentUser"/> üzerinden
/// verir. Böylece authentication provider değişse dahi policy davranışı sabit
/// kalır.
/// </summary>
public sealed class RegisteredUserAuthorizationHandler
    : AuthorizationHandler<RegisteredUserRequirement>
{
    private readonly ICurrentUser _currentUser;

    public RegisteredUserAuthorizationHandler(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        RegisteredUserRequirement requirement)
    {
        if (_currentUser is { IsAuthenticated: true, IsRegistered: true, IsActive: true })
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

public sealed class CompanyAuthorizationHandler
    : AuthorizationHandler<CompanyRequirement>
{
    private readonly ICurrentUser _currentUser;
    private readonly ICompanyContext _companyContext;

    public CompanyAuthorizationHandler(
        ICurrentUser currentUser,
        ICompanyContext companyContext)
    {
        _currentUser = currentUser;
        _companyContext = companyContext;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CompanyRequirement requirement)
    {
        if (_currentUser is { IsRegistered: true, IsActive: true }
            && _companyContext.HasCompany)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

public sealed class RoleAuthorizationHandler
    : AuthorizationHandler<RoleRequirement>
{
    private readonly ICurrentUser _currentUser;

    public RoleAuthorizationHandler(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        RoleRequirement requirement)
    {
        if (_currentUser is { IsActive: true }
            && requirement.AllowedRoles.Any(_currentUser.IsInRole))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

public sealed class PermissionAuthorizationHandler
    : AuthorizationHandler<PermissionRequirement>
{
    private readonly ICurrentUser _currentUser;

    public PermissionAuthorizationHandler(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (_currentUser is { IsActive: true }
            && _currentUser.HasPermission(requirement.Permission))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
