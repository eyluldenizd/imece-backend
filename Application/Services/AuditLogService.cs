using Application.DTOs;
using Core.Common;
using Infrastructure.Entities;
using Infrastructure.Repositories;

namespace Application.Services;

public sealed class AuditLogService
{
    private readonly AuditLogRepository _repository;

    public AuditLogService(AuditLogRepository repository)
    {
        _repository = repository;
    }

    public async Task<ServiceResult<PagedResultDto<AuditLogDto>>> GetPagedAsync(
        AuditLogListQueryDto? query = null,
        CancellationToken cancellationToken = default)
    {
        var page = query?.Page is int p && p > 0 ? p : 1;
        var pageSize = query?.PageSize is int s && s > 0 ? Math.Min(s, 100) : 20;
        var filter = ToFilter(query);
        var orderBy = ResolveOrderBy(query?.SortBy, query?.SortDir);

        var totalCount = await _repository.CountFilteredAsync(filter, cancellationToken);
        var totalPages = totalCount == 0
            ? 0
            : (int)Math.Ceiling(totalCount / (double)pageSize);

        if (page > totalPages && totalPages > 0)
        {
            page = totalPages;
        }

        var offset = (page - 1) * pageSize;
        var rows = totalCount == 0
            ? []
            : await _repository.GetPagedAsync(filter, offset, pageSize, orderBy, cancellationToken);

        return ServiceResult<PagedResultDto<AuditLogDto>>.Success(new PagedResultDto<AuditLogDto>
        {
            Items = rows.Select(ToDto).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
        });
    }

    public async Task<ServiceResult<AuditLogDto>> GetByIdAsync(
        IdRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (entity is null)
        {
            return ServiceResult<AuditLogDto>.NotFound("Denetim kaydı bulunamadı.");
        }

        return ServiceResult<AuditLogDto>.Success(ToDto(entity));
    }

    public async Task<ServiceResult<AuditLogFilterOptionsDto>> GetFilterOptionsAsync(
        CancellationToken cancellationToken = default)
    {
        var users = await _repository.GetDistinctUserNamesAsync(cancellationToken);
        var categories = await _repository.GetDistinctCategoriesAsync(cancellationToken);
        var entityTypes = await _repository.GetDistinctEntityTypesAsync(cancellationToken);

        return ServiceResult<AuditLogFilterOptionsDto>.Success(new AuditLogFilterOptionsDto
        {
            Users = users,
            Categories = categories,
            EntityTypes = entityTypes,
        });
    }

    private static AuditLogListFilter ToFilter(AuditLogListQueryDto? query) => new()
    {
        Search = query?.Search,
        DateFrom = query?.DateFrom,
        DateTo = query?.DateTo,
        UserName = query?.UserName,
        Category = query?.Category,
        Outcome = query?.Outcome,
        EntityType = query?.EntityType,
        ClientIp = query?.ClientIp,
    };

    /// <summary>Whitelist only — never interpolate raw client sort text into SQL.</summary>
    private static string ResolveOrderBy(string? sortBy, string? sortDir)
    {
        var ascending = string.Equals(sortDir, "asc", StringComparison.OrdinalIgnoreCase);
        var dir = ascending ? "ASC" : "DESC";

        return (sortBy?.Trim().ToLowerInvariant()) switch
        {
            "username" or "user" => $"u.full_name {dir}, a.occurred_at DESC",
            "action" => $"a.action {dir}, a.occurred_at DESC",
            "occurredat" or "occurred_at" or "date" or "time" => $"a.occurred_at {dir}",
            _ => "a.occurred_at DESC",
        };
    }

    private static AuditLogDto ToDto(AuditLogs entity) => new()
    {
        AuditId = entity.AuditId,
        OccurredAt = entity.OccurredAt,
        Action = entity.Action,
        Category = entity.Category,
        Outcome = entity.Outcome,
        EntityType = entity.EntityType,
        EntityId = entity.EntityId,
        UserId = entity.UserId,
        UserName = entity.UserName,
        CompanyId = entity.CompanyId,
        CompanyName = entity.CompanyName,
        TraceId = entity.TraceId,
        ClientIp = entity.ClientIp,
        UserAgent = entity.UserAgent,
        ClientApplication = entity.ClientApplication,
        HttpMethod = entity.HttpMethod,
        RequestPath = entity.RequestPath,
        StatusCode = entity.StatusCode,
        DurationMs = entity.DurationMs,
        ErrorCode = entity.ErrorCode,
        ExceptionType = entity.ExceptionType,
        BeforeJson = entity.BeforeJson,
        AfterJson = entity.AfterJson,
        RequestBodyJson = entity.RequestBodyJson,
    };
}
