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

    public static IEnumerable<T> ApplyMultiFieldSearch<T>(
        IEnumerable<T> source,
        ContentListQueryDto? query,
        params Func<T, string?>[] getters)
    {
        if (string.IsNullOrWhiteSpace(query?.Search) || getters.Length == 0)
        {
            return source;
        }

        var search = query.Search.Trim();
        return source.Where(item =>
            getters.Any(getter => ContainsIgnoreCase(getter(item), search)));
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

    public static IEnumerable<T> ApplyDepartmentId<T>(
        IEnumerable<T> source,
        ContentListQueryDto? query,
        Func<T, int?> getDepartmentId)
    {
        if (query?.DepartmentId is not int departmentId)
        {
            return source;
        }

        return source.Where(item => getDepartmentId(item) == departmentId);
    }

    public static IEnumerable<T> ApplyRoleName<T>(
        IEnumerable<T> source,
        ContentListQueryDto? query,
        Func<T, string?> getRoleName)
    {
        if (string.IsNullOrWhiteSpace(query?.RoleName))
        {
            return source;
        }

        var roleName = query.RoleName.Trim();
        return source.Where(item =>
            string.Equals(getRoleName(item)?.Trim(), roleName, StringComparison.OrdinalIgnoreCase));
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

    public static IEnumerable<T> ApplyReservationRecordActive<T>(
        IEnumerable<T> source,
        ContentListQueryDto? query,
        Func<T, string> getStatus)
    {
        if (query?.IsActive is not bool isActive)
        {
            return source;
        }

        return source.Where(item =>
        {
            var cancelled = string.Equals(getStatus(item), "cancelled", StringComparison.OrdinalIgnoreCase);
            return isActive ? !cancelled : cancelled;
        });
    }

    public static IEnumerable<T> ApplyStatus<T>(
        IEnumerable<T> source,
        ContentListQueryDto? query,
        Func<T, string?> getStatus)
    {
        if (string.IsNullOrWhiteSpace(query?.Status))
        {
            return source;
        }

        var status = query.Status.Trim();
        return source.Where(item =>
            string.Equals(getStatus(item)?.Trim(), status, StringComparison.OrdinalIgnoreCase));
    }

    public static IEnumerable<T> ApplyEventLifecycleStatus<T>(
        IEnumerable<T> source,
        ContentListQueryDto? query,
        Func<T, DateTime> getStart,
        Func<T, DateTime> getEnd)
    {
        if (string.IsNullOrWhiteSpace(query?.Status))
        {
            return source;
        }

        var status = query.Status.Trim();
        if (!status.Equals("upcoming", StringComparison.OrdinalIgnoreCase)
            && !status.Equals("ongoing", StringComparison.OrdinalIgnoreCase)
            && !status.Equals("completed", StringComparison.OrdinalIgnoreCase))
        {
            return source;
        }

        var now = DateTime.Now;
        return source.Where(item =>
            string.Equals(
                EventLifecycleHelper.GetStatus(getStart(item), getEnd(item), now),
                status,
                StringComparison.OrdinalIgnoreCase));
    }

    public static IEnumerable<T> ApplyCategoryId<T>(
        IEnumerable<T> source,
        ContentListQueryDto? query,
        Func<T, int?> getCategoryId)
    {
        if (query?.CategoryId is not int categoryId)
        {
            return source;
        }

        return source.Where(item => getCategoryId(item) == categoryId);
    }

    public static IEnumerable<T> ApplyType<T>(
        IEnumerable<T> source,
        ContentListQueryDto? query,
        Func<T, string?> getType)
    {
        if (string.IsNullOrWhiteSpace(query?.Type))
        {
            return source;
        }

        var type = query.Type.Trim();
        return source.Where(item =>
            string.Equals(getType(item)?.Trim(), type, StringComparison.OrdinalIgnoreCase));
    }

    public static IEnumerable<T> ApplyStringContains<T>(
        IEnumerable<T> source,
        ContentListQueryDto? query,
        string? queryValue,
        Func<T, string?> getValue)
    {
        if (string.IsNullOrWhiteSpace(queryValue))
        {
            return source;
        }

        var needle = queryValue.Trim();
        return source.Where(item => ContainsIgnoreCase(getValue(item), needle));
    }

    public static IEnumerable<T> ApplyExactString<T>(
        IEnumerable<T> source,
        ContentListQueryDto? query,
        string? queryValue,
        Func<T, string?> getValue)
    {
        if (string.IsNullOrWhiteSpace(queryValue))
        {
            return source;
        }

        var needle = queryValue.Trim();
        return source.Where(item =>
            string.Equals(getValue(item)?.Trim(), needle, StringComparison.OrdinalIgnoreCase));
    }

    public static IEnumerable<T> ApplyIsPublished<T>(
        IEnumerable<T> source,
        ContentListQueryDto? query,
        Func<T, bool> getIsPublished)
    {
        if (query?.IsPublished is not bool isPublished)
        {
            return source;
        }

        return source.Where(item => getIsPublished(item) == isPublished);
    }

    public static IEnumerable<T> ApplyFeatureType<T>(
        IEnumerable<T> source,
        ContentListQueryDto? query,
        Func<T, string?> getFeatureType)
    {
        if (string.IsNullOrWhiteSpace(query?.FeatureType))
        {
            return source;
        }

        var featureType = query.FeatureType.Trim();
        return source.Where(item =>
            string.Equals(getFeatureType(item)?.Trim(), featureType, StringComparison.OrdinalIgnoreCase));
    }

    public static IEnumerable<T> ApplyMeetingRoomId<T>(
        IEnumerable<T> source,
        ContentListQueryDto? query,
        Func<T, int?> getMeetingRoomId)
    {
        if (query?.MeetingRoomId is not int meetingRoomId)
        {
            return source;
        }

        return source.Where(item => getMeetingRoomId(item) == meetingRoomId);
    }

    public static IEnumerable<T> ApplyYear<T>(
        IEnumerable<T> source,
        ContentListQueryDto? query,
        Func<T, int> getYear)
    {
        if (query?.Year is not int year)
        {
            return source;
        }

        return source.Where(item => getYear(item) == year);
    }

    public static IEnumerable<T> ApplyMonth<T>(
        IEnumerable<T> source,
        ContentListQueryDto? query,
        Func<T, int> getMonth)
    {
        if (query?.Month is not int month)
        {
            return source;
        }

        return source.Where(item => getMonth(item) == month);
    }

    public static IEnumerable<MediaFileDto> ApplyMediaDocumentType(
        IEnumerable<MediaFileDto> source,
        ContentListQueryDto? query)
    {
        if (string.IsNullOrWhiteSpace(query?.Type))
        {
            return source;
        }

        var type = query.Type.Trim();
        return source.Where(file =>
        {
            var docType = ResolveDocumentType(file.ContentType, file.OriginalFileName, file.FileExtension);
            return string.Equals(docType, type, StringComparison.OrdinalIgnoreCase);
        });
    }

    private static string ResolveDocumentType(string contentType, string originalFileName, string fileExtension)
    {
        var mime = contentType?.Trim().ToLowerInvariant() ?? string.Empty;
        var name = originalFileName?.Trim().ToLowerInvariant() ?? string.Empty;
        var ext = fileExtension?.Trim().ToLowerInvariant() ?? string.Empty;

        if (mime.Contains("pdf") || name.EndsWith(".pdf") || ext == ".pdf" || ext == "pdf")
        {
            return "pdf";
        }

        if (mime.Contains("word") || name.EndsWith(".doc") || name.EndsWith(".docx")
            || ext is ".doc" or ".docx" or "doc" or "docx")
        {
            return "doc";
        }

        return "other";
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

    public static IEnumerable<AnnouncementDto> ApplyPublishWindowActive(
        IEnumerable<AnnouncementDto> source,
        ContentListQueryDto? query)
    {
        if (query?.IsActive is not bool isActive)
        {
            return source;
        }

        var today = DateTime.UtcNow.Date;
        return source.Where(item =>
        {
            var inWindow = item.PublishStart.Date <= today
                && (item.PublishEnd is null || item.PublishEnd.Value.Date >= today);
            return inWindow == isActive;
        });
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

    /// <summary>
    /// Applies Skip/Take after filter+sort. Defaults: page=1, pageSize=20 (max 100).
    /// </summary>
    public static PagedResultDto<T> ApplyPaging<T>(
        IReadOnlyList<T> source,
        ContentListQueryDto? query)
    {
        var page = query?.Page is int p && p > 0 ? p : 1;
        var pageSize = query?.PageSize is int s && s > 0 ? Math.Min(s, 100) : 20;
        var totalCount = source.Count;
        var totalPages = totalCount == 0
            ? 0
            : (int)Math.Ceiling(totalCount / (double)pageSize);

        if (page > totalPages && totalPages > 0)
        {
            page = totalPages;
        }

        var items = source
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedResultDto<T>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
        };
    }

    private static bool ContainsIgnoreCase(string? value, string search) =>
        !string.IsNullOrEmpty(value)
        && value.Contains(search, StringComparison.OrdinalIgnoreCase);
}
