using Application.DTOs;
using Application.Exceptions;
using Core.Authorization;
using Infrastructure.Repositories;

namespace Application.Common.OrganizationScope;

public sealed class OrganizationScopeService
{
    private readonly ICompanyContext _companyContext;
    private readonly BranchRepository _branchRepository;
    private readonly DepartmentRepository _departmentRepository;

    public OrganizationScopeService(
        ICompanyContext companyContext,
        BranchRepository branchRepository,
        DepartmentRepository departmentRepository)
    {
        _companyContext = companyContext;
        _branchRepository = branchRepository;
        _departmentRepository = departmentRepository;
    }

    public async Task<(ResolvedOrganizationScope? Resolved, string? ErrorMessage)> ResolveAsync(
        OrganizationScopeFieldsDto request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var scope = OrganizationScopeFieldHelper.ToScope(
                request.CompanyScope,
                request.CompanyId,
                request.BranchScope,
                request.BranchId,
                request.DepartmentScope,
                request.DepartmentId);

            var resolved = await OrganizationScopeRules.ResolveAsync(
                scope,
                _companyContext,
                _branchRepository,
                _departmentRepository,
                cancellationToken);

            return (resolved, null);
        }
        catch (ForbiddenException)
        {
            throw;
        }
        catch (InvalidOperationException ex)
        {
            return (null, ex.Message);
        }
    }

    public static void ApplyToEntity(
        ResolvedOrganizationScope resolved,
        Action<string, int?, string, int?, string, int?> apply)
    {
        OrganizationScopeFieldHelper.ApplyResolved(resolved, apply);
    }
}
