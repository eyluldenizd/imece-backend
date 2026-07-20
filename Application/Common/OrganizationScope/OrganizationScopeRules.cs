using Application.Exceptions;
using Core.Authorization;
using Infrastructure.Repositories;

namespace Application.Common.OrganizationScope;

public static class OrganizationScopeRules
{
    public static bool TryResolve(
        OrganizationScope scope,
        ICompanyContext companyContext,
        BranchRepository branchRepository,
        DepartmentRepository departmentRepository,
        out ResolvedOrganizationScope resolved,
        out string errorMessage,
        out bool isForbidden)
    {
        resolved = default!;
        errorMessage = string.Empty;
        isForbidden = false;

        try
        {
            resolved = ResolveAsync(
                    scope,
                    companyContext,
                    branchRepository,
                    departmentRepository,
                    CancellationToken.None)
                .GetAwaiter()
                .GetResult();

            return true;
        }
        catch (ForbiddenException ex)
        {
            errorMessage = ex.Message;
            isForbidden = true;
            return false;
        }
        catch (InvalidOperationException ex)
        {
            errorMessage = ex.Message;
            return false;
        }
    }

    public static async Task<ResolvedOrganizationScope> ResolveAsync(
        OrganizationScope scope,
        ICompanyContext companyContext,
        BranchRepository branchRepository,
        DepartmentRepository departmentRepository,
        CancellationToken cancellationToken = default)
    {
        ValidateScopeShape(scope);

        if (scope.CompanyScope == OrganizationScopeLevel.All)
        {
            return new ResolvedOrganizationScope(
                OrganizationScopeLevel.All,
                null,
                OrganizationScopeLevel.All,
                null,
                OrganizationScopeLevel.All,
                null);
        }

        var companyId = scope.CompanyId
            ?? throw new InvalidOperationException("Şirket kapsamı Specific iken companyId zorunludur.");

        companyContext.EnsureCanAccessCompany(companyId);

        if (scope.BranchScope == OrganizationScopeLevel.All)
        {
            return new ResolvedOrganizationScope(
                OrganizationScopeLevel.Specific,
                companyId,
                OrganizationScopeLevel.All,
                null,
                OrganizationScopeLevel.All,
                null);
        }

        var branchId = scope.BranchId
            ?? throw new InvalidOperationException("Şube kapsamı Specific iken branchId zorunludur.");

        var branch = await branchRepository.GetByIdAsync(branchId, cancellationToken)
            ?? throw new InvalidOperationException("Geçersiz şube ID değeri.");

        if (!branch.CompanyId.HasValue || branch.CompanyId.Value != companyId)
        {
            throw new InvalidOperationException(
                "Seçilen şube belirtilen şirkete ait değil.");
        }

        if (scope.DepartmentScope == OrganizationScopeLevel.All)
        {
            return new ResolvedOrganizationScope(
                OrganizationScopeLevel.Specific,
                companyId,
                OrganizationScopeLevel.Specific,
                branchId,
                OrganizationScopeLevel.All,
                null);
        }

        var departmentId = scope.DepartmentId
            ?? throw new InvalidOperationException("Departman kapsamı Specific iken departmentId zorunludur.");

        var department = await departmentRepository.GetByIdAsync(departmentId, cancellationToken)
            ?? throw new InvalidOperationException("Geçersiz departman ID değeri.");

        if (!department.BranchId.HasValue || department.BranchId.Value != branchId)
        {
            throw new InvalidOperationException(
                "Seçilen departman belirtilen şubeye ait değil.");
        }

        if (department.CompanyId.HasValue && department.CompanyId.Value != companyId)
        {
            throw new InvalidOperationException(
                "Seçilen departman belirtilen şirkete ait değil.");
        }

        return new ResolvedOrganizationScope(
            OrganizationScopeLevel.Specific,
            companyId,
            OrganizationScopeLevel.Specific,
            branchId,
            OrganizationScopeLevel.Specific,
            departmentId);
    }

    private static void ValidateScopeShape(OrganizationScope scope)
    {
        if (scope.CompanyScope == OrganizationScopeLevel.All)
        {
            if (scope.CompanyId.HasValue)
            {
                throw new InvalidOperationException(
                    "Tüm şirketler kapsamında companyId belirtilmemelidir.");
            }

            if (scope.BranchScope == OrganizationScopeLevel.Specific
                || scope.DepartmentScope == OrganizationScopeLevel.Specific)
            {
                throw new InvalidOperationException(
                    "Tüm şirketler kapsamında şube ve departman kapsamı All olmalıdır.");
            }

            if (scope.BranchId.HasValue || scope.DepartmentId.HasValue)
            {
                throw new InvalidOperationException(
                    "Tüm şirketler kapsamında branchId ve departmentId null olmalıdır.");
            }

            return;
        }

        if (!scope.CompanyId.HasValue || scope.CompanyId.Value <= 0)
        {
            throw new InvalidOperationException(
                "Şirket kapsamı Specific iken geçerli bir companyId zorunludur.");
        }

        if (scope.BranchScope == OrganizationScopeLevel.All)
        {
            if (scope.BranchId.HasValue)
            {
                throw new InvalidOperationException(
                    "Tüm şubeler kapsamında branchId null olmalıdır.");
            }

            if (scope.DepartmentScope == OrganizationScopeLevel.Specific)
            {
                throw new InvalidOperationException(
                    "Tüm şubeler kapsamında departman kapsamı All olmalıdır.");
            }

            if (scope.DepartmentId.HasValue)
            {
                throw new InvalidOperationException(
                    "Tüm şubeler kapsamında departmentId null olmalıdır.");
            }

            return;
        }

        if (!scope.BranchId.HasValue || scope.BranchId.Value <= 0)
        {
            throw new InvalidOperationException(
                "Şube kapsamı Specific iken geçerli bir branchId zorunludur.");
        }

        if (scope.DepartmentScope == OrganizationScopeLevel.All)
        {
            if (scope.DepartmentId.HasValue)
            {
                throw new InvalidOperationException(
                    "Tüm departmanlar kapsamında departmentId null olmalıdır.");
            }

            return;
        }

        if (!scope.DepartmentId.HasValue || scope.DepartmentId.Value <= 0)
        {
            throw new InvalidOperationException(
                "Departman kapsamı Specific iken geçerli bir departmentId zorunludur.");
        }
    }
}
