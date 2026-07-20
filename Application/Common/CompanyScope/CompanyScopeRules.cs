using Application.Exceptions;
using Core.Authorization;
using Infrastructure.Repositories;

namespace Application.Common.CompanyScope;

public sealed record ResolvedContentScope(string ScopeType, int? CompanyId);

public static class CompanyScopeRules
{
    public static CompanyListFilter ResolveListCompanyFilter(
        ICompanyContext companyContext,
        ICurrentUser currentUser)
    {
        if (companyContext.IsGlobalAdmin)
        {
            return new CompanyListFilter(null, null);
        }

        var membershipIds = currentUser.CompanyMemberships
            .Select(membership => membership.CompanyId)
            .Distinct()
            .ToArray();

        if (membershipIds.Length == 1)
        {
            return new CompanyListFilter(membershipIds[0], null);
        }

        if (membershipIds.Length > 1)
        {
            return new CompanyListFilter(null, string.Join(',', membershipIds));
        }

        if (companyContext.CurrentCompanyId.HasValue)
        {
            return new CompanyListFilter(companyContext.CurrentCompanyId.Value, null);
        }

        throw new ForbiddenException("Bu işlem bir şirkete bağlı kullanıcı gerektirir.");
    }

    public static bool TryResolveScope(
        ICompanyContext companyContext,
        ICurrentUser currentUser,
        string? requestedScopeType,
        int? requestedCompanyId,
        out ResolvedContentScope scope,
        out string errorMessage)
    {
        try
        {
            scope = ResolveScope(
                companyContext,
                currentUser,
                requestedScopeType,
                requestedCompanyId);
            errorMessage = string.Empty;
            return true;
        }
        catch (ForbiddenException ex)
        {
            scope = default!;
            errorMessage = ex.Message;
            return false;
        }
    }

    public static ResolvedContentScope ResolveScope(
        ICompanyContext companyContext,
        ICurrentUser currentUser,
        string? requestedScopeType,
        int? requestedCompanyId)
    {
        var scopeType = NormalizeScopeType(requestedScopeType);

        if (scopeType.Equals(ContentScopeTypes.Global, StringComparison.OrdinalIgnoreCase))
        {
            if (!CanManageGlobalContent(companyContext, currentUser))
            {
                throw new ForbiddenException(
                    "Global kapsamlı içerik yönetmek için global yönetici veya content.global.manage izni gerekir.");
            }

            return new ResolvedContentScope(ContentScopeTypes.Global, null);
        }

        if (!requestedCompanyId.HasValue || requestedCompanyId.Value <= 0)
        {
            throw new ForbiddenException(
                "Şirket kapsamlı içerik için hedef şirket belirtilmelidir.");
        }

        companyContext.EnsureCanAccessCompany(requestedCompanyId.Value);
        return new ResolvedContentScope(ContentScopeTypes.Company, requestedCompanyId.Value);
    }

    public static bool CanManageGlobalContent(
        ICompanyContext companyContext,
        ICurrentUser currentUser) =>
        companyContext.IsGlobalAdmin
        || currentUser.HasPermission(Permissions.ContentGlobalManage);

    public static void EnsureCompanyAccess(
        ICompanyContext companyContext,
        int? recordCompanyId)
    {
        if (!recordCompanyId.HasValue)
        {
            if (companyContext.IsGlobalAdmin)
            {
                return;
            }

            throw new ForbiddenException(
                "Bu şirkete ait veriye erişim yetkiniz bulunmuyor.");
        }

        companyContext.EnsureCanAccessCompany(recordCompanyId.Value);
    }

    public static void EnsureContentReadAccess(
        ICompanyContext companyContext,
        string scopeType,
        int? recordCompanyId)
    {
        if (scopeType.Equals(ContentScopeTypes.Global, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        EnsureCompanyAccess(companyContext, recordCompanyId);
    }

    public static void EnsureContentWriteAccess(
        ICompanyContext companyContext,
        ICurrentUser currentUser,
        string scopeType,
        int? recordCompanyId)
    {
        if (scopeType.Equals(ContentScopeTypes.Global, StringComparison.OrdinalIgnoreCase))
        {
            if (!CanManageGlobalContent(companyContext, currentUser))
            {
                throw new ForbiddenException(
                    "Global kapsamlı içerik yönetmek için global yönetici veya content.global.manage izni gerekir.");
            }

            return;
        }

        EnsureCompanyAccess(companyContext, recordCompanyId);
    }

    private static string NormalizeScopeType(string? requestedScopeType)
    {
        if (string.IsNullOrWhiteSpace(requestedScopeType))
        {
            return ContentScopeTypes.Company;
        }

        var normalized = requestedScopeType.Trim();
        if (normalized.Equals(ContentScopeTypes.Global, StringComparison.OrdinalIgnoreCase))
        {
            return ContentScopeTypes.Global;
        }

        if (normalized.Equals(ContentScopeTypes.Company, StringComparison.OrdinalIgnoreCase))
        {
            return ContentScopeTypes.Company;
        }

        throw new ForbiddenException("Geçersiz scope_type değeri. Company veya Global olmalıdır.");
    }
}
