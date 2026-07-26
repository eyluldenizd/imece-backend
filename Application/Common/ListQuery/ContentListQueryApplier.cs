using Application.DTOs;

namespace Application.Common.ListQuery;

public static class ContentListQueryApplier
{
    public static IEnumerable<T> ApplySearch<T>(
        IEnumerable<T> source,
        ContentListQueryDto? query,
        Func<T, string> getPrimary,
        Func<T, string>? getSecondary = null)
    {
        if (string.IsNullOrWhiteSpace(query?.Search))
        {
            return source;
        }

        var search = query.Search.Trim();
        return source.Where(item =>
            ContainsIgnoreCase(getPrimary(item), search)
            || (getSecondary != null && ContainsIgnoreCase(getSecondary(item), search)));
    }

    public static IEnumerable<T> ApplyCompanyId<T>(
        IEnumerable<T> source,
        ContentListQueryDto? query,
        Func<T, int?> getCompanyId)
    {
        if (query?.CompanyId is not int companyId)
        {
            return source;
        }

        return source.Where(item => getCompanyId(item) == companyId);
    }

    public static IEnumerable<T> ApplyCompanyName<T>(
        IEnumerable<T> source,
        ContentListQueryDto? query,
        Func<T, string?> getCompanyName)
    {
        if (string.IsNullOrWhiteSpace(query?.CompanyName))
        {
            return source;
        }

        var companyName = query.CompanyName.Trim();
        return source.Where(item =>
            string.Equals(getCompanyName(item)?.Trim(), companyName, StringComparison.OrdinalIgnoreCase));
    }

    public static IEnumerable<T> ApplyBranchId<T>(
        IEnumerable<T> source,
        ContentListQueryDto? query,
        Func<T, int?> getBranchId)
    {
        if (query?.BranchId is not int branchId)
        {
            return source;
        }

        return source.Where(item => getBranchId(item) == branchId);
    }

    public static IEnumerable<T> ApplyIsPinned<T>(
        IEnumerable<T> source,
        ContentListQueryDto? query,
        Func<T, bool> getIsPinned)
    {
        if (query?.IsPinned is not bool isPinned)
        {
            return source;
        }

        return source.Where(item => getIsPinned(item) == isPinned);
    }

    public static IEnumerable<T> ApplyIsActive<T>(
        IEnumerable<T> source,
        ContentListQueryDto? query,
        Func<T, bool> getIsActive)
    {
        if (query?.IsActive is not bool isActive)
        {
            return source;
        }

        return source.Where(item => getIsActive(item) == isActive);
    }

    public static IEnumerable<T> ApplyScopeType<T>(
        IEnumerable<T> source,
        ContentListQueryDto? query,
        Func<T, string> getScopeType)
    {
        if (string.IsNullOrWhiteSpace(query?.ScopeType))
        {
            return source;
        }

        var scopeType = query.ScopeType.Trim();
        return source.Where(item =>
            string.Equals(getScopeType(item), scopeType, StringComparison.OrdinalIgnoreCase));
    }

    public static IEnumerable<T> ApplyDateRange<T>(
        IEnumerable<T> source,
        ContentListQueryDto? query,
        Func<T, DateTime> getDate)
    {
        IEnumerable<T> result = source;

        if (query?.DateFrom is DateTime dateFrom)
        {
            var fromDate = dateFrom.Date;
            result = result.Where(item => getDate(item).Date >= fromDate);
        }

        if (query?.DateTo is DateTime dateTo)
        {
            var toDate = dateTo.Date;
            result = result.Where(item => getDate(item).Date <= toDate);
        }

        return result;
    }

    public static IReadOnlyList<T> ApplySort<T>(
        IEnumerable<T> source,
        ContentListQueryDto? query,
        IReadOnlyDictionary<string, Func<T, IComparable>> sortKeys,
        Func<IEnumerable<T>, IOrderedEnumerable<T>> defaultSort)
    {
        var sortBy = query?.SortBy?.Trim();
        if (string.IsNullOrWhiteSpace(sortBy)
            || !sortKeys.TryGetValue(sortBy, out var keySelector))
        {
            return defaultSort(source).ToList();
        }

        var descending = string.Equals(query?.SortDir, "desc", StringComparison.OrdinalIgnoreCase);
        return descending
            ? source.OrderByDescending(keySelector).ToList()
            : source.OrderBy(keySelector).ToList();
    }

    private static bool ContainsIgnoreCase(string? value, string search) =>
        !string.IsNullOrEmpty(value)
        && value.Contains(search, StringComparison.OrdinalIgnoreCase);
}
