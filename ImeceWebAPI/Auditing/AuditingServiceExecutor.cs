using System.Diagnostics;
using System.Reflection;
using Core.Auditing;
using Core.Common;
using Core.Common.Execution;
using Infrastructure.Database.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ImeceWebAPI.Auditing;

/// <summary>
/// <see cref="IServiceExecutor"/> dekoratörü.
/// Create/Update/Delete benzeri işlemleri otomatik mutation audit'e yazar.
/// SOLID: tek sorumluluk = executor sonucu + denetim; iş mantığına dokunmaz.
/// </summary>
public sealed class AuditingServiceExecutor : IServiceExecutor
{
    private readonly IServiceExecutor _inner;
    private readonly IAuditService _auditService;
    private readonly IAuditRequestContext _requestContext;
    private readonly IOptions<AuditOptions> _options;
    private readonly ILogger<AuditingServiceExecutor> _logger;

    public AuditingServiceExecutor(
        IServiceExecutor inner,
        IAuditService auditService,
        IAuditRequestContext requestContext,
        IOptions<AuditOptions> options,
        ILogger<AuditingServiceExecutor> logger)
    {
        _inner = inner;
        _auditService = auditService;
        _requestContext = requestContext;
        _options = options;
        _logger = logger;
    }

    public async Task<ServiceResult<TResponse>> ExecuteAsync<TResponse>(
        Func<CancellationToken, Task<ServiceResult<TResponse>>> serviceMethod,
        CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();
        var result = await _inner.ExecuteAsync(serviceMethod, cancellationToken);
        sw.Stop();

        // Tipik GetAll/GetById — mutation değil; HTTP middleware kapsar.
        return result;
    }

    public async Task<ServiceResult<TResponse>> ExecuteAsync<TRequest, TResponse>(
        TRequest request,
        Func<TRequest, CancellationToken, Task<ServiceResult<TResponse>>> serviceMethod,
        CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();
        var result = await _inner.ExecuteAsync(request, serviceMethod, cancellationToken);
        sw.Stop();

        await TryAuditMutationAsync(
            typeof(TRequest),
            request,
            result.StatusCode,
            result.Message,
            result.Data,
            sw.ElapsedMilliseconds,
            cancellationToken);

        return result;
    }

    public async Task<ServiceResult> ExecuteAsync(
        Func<CancellationToken, Task<ServiceResult>> serviceMethod,
        CancellationToken cancellationToken)
    {
        return await _inner.ExecuteAsync(serviceMethod, cancellationToken);
    }

    public async Task<ServiceResult> ExecuteAsync<TRequest>(
        TRequest request,
        Func<TRequest, CancellationToken, Task<ServiceResult>> serviceMethod,
        CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();
        var result = await _inner.ExecuteAsync(request, serviceMethod, cancellationToken);
        sw.Stop();

        await TryAuditMutationAsync(
            typeof(TRequest),
            request,
            result.StatusCode,
            result.Message,
            after: null,
            sw.ElapsedMilliseconds,
            cancellationToken);

        return result;
    }

    private async Task TryAuditMutationAsync(
        Type requestType,
        object? request,
        StatusCodeEnum statusCode,
        string? message,
        object? after,
        long durationMs,
        CancellationToken cancellationToken)
    {
        var options = _options.Value;
        if (!options.Enabled || !options.CaptureMutations)
        {
            return;
        }

        if (!AuditMutationClassifier.TryClassify(
                requestType.Name,
                _requestContext.HttpMethod,
                out var actionVerb,
                out var entityType))
        {
            return;
        }

        var outcome = MapOutcome(statusCode);
        if (outcome != AuditOutcomes.Success && !options.CaptureFailedResults)
        {
            return;
        }

        try
        {
            await _auditService.WriteAsync(
                new AuditEvent
                {
                    Action = $"{entityType}.{actionVerb}",
                    Category = AuditCategories.Mutation,
                    Outcome = outcome,
                    EntityType = entityType,
                    EntityId = TryExtractEntityId(request),
                    HttpMethod = _requestContext.HttpMethod,
                    RequestPath = _requestContext.RequestPath,
                    StatusCode = MapHttpStatus(statusCode),
                    DurationMs = durationMs,
                    ErrorCode = outcome == AuditOutcomes.Success ? null : statusCode.ToString(),
                    After = after ?? request,
                    RequestBody = request
                },
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Mutation audit yazılamadı: {Entity}.{Verb}", entityType, actionVerb);
        }
    }

    private static string MapOutcome(StatusCodeEnum statusCode) =>
        statusCode switch
        {
            StatusCodeEnum.Success or StatusCodeEnum.Created or StatusCodeEnum.NoContent
                => AuditOutcomes.Success,
            _ => AuditOutcomes.Failure
        };

    private static int MapHttpStatus(StatusCodeEnum statusCode) =>
        statusCode switch
        {
            StatusCodeEnum.Success => 200,
            StatusCodeEnum.Created => 201,
            StatusCodeEnum.NoContent => 204,
            StatusCodeEnum.BadRequest => 400,
            StatusCodeEnum.NotFound => 404,
            StatusCodeEnum.Conflict => 409,
            _ => 500
        };

    private static string? TryExtractEntityId(object? request)
    {
        if (request is null)
        {
            return null;
        }

        foreach (var name in new[] { "Id", "EntityId" })
        {
            var prop = request.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (prop?.CanRead == true)
            {
                var value = prop.GetValue(request);
                if (value is not null)
                {
                    return value.ToString();
                }
            }
        }

        var idProp = request.GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(p =>
                p.CanRead
                && p.Name.EndsWith("Id", StringComparison.OrdinalIgnoreCase)
                && p.Name.Length > 2);

        return idProp?.GetValue(request)?.ToString();
    }
}
