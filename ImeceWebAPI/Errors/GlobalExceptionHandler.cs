using System.Diagnostics;
using Application.Exceptions;
using Core.Auditing;
using Infrastructure.Database.Options;
using ImeceWebAPI.Options;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ImeceWebAPI.Errors;

/// <summary>
/// Uygulamadaki tek merkezî exception → ProblemDetails dönüştürme ve
/// loglama noktası. Beklenen (AppException) hatalar güvenli mesajla ilgili
/// HTTP status'una çevrilir; beklenmeyen hatalar 500 olarak, iç ayrıntılar
/// client'a sızdırılmadan raporlanır. Audit Enabled ise DB'ye de yazar.
/// </summary>
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    // 499 Client Closed Request (StatusCodes içinde sabit yok).
    private const int ClientClosedRequest = 499;

    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _environment;
    private readonly ExceptionHandlingOptions _options;
    private readonly IOptions<AuditOptions> _auditOptions;
    private readonly IServiceScopeFactory _scopeFactory;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger,
        IHostEnvironment environment,
        IOptions<ExceptionHandlingOptions> options,
        IOptions<AuditOptions> auditOptions,
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _environment = environment;
        _options = options.Value;
        _auditOptions = auditOptions;
        _scopeFactory = scopeFactory;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // Client bağlantıyı kapattıysa bu bir sunucu hatası değildir.
        if (exception is OperationCanceledException
            && httpContext.RequestAborted.IsCancellationRequested)
        {
            _logger.LogInformation(
                "Request was canceled by the client. "
                + "Method: {RequestMethod}, Path: {RequestPath}",
                httpContext.Request.Method,
                httpContext.Request.Path);

            if (!httpContext.Response.HasStarted)
            {
                httpContext.Response.StatusCode = ClientClosedRequest;
            }

            return true;
        }

        // Response gövdesi yazılmaya başladıysa yeniden yazamayız.
        if (httpContext.Response.HasStarted)
        {
            _logger.LogWarning(
                exception,
                "The response has already started; the exception cannot be "
                + "converted to a ProblemDetails result. Path: {RequestPath}",
                httpContext.Request.Path);

            return false;
        }

        var problemDetails = exception is AppException appException
            ? HandleApplicationException(httpContext, appException)
            : HandleUnexpectedException(httpContext, exception);

        await TryWriteAuditAsync(httpContext, exception, problemDetails, cancellationToken);

        httpContext.Response.StatusCode =
            problemDetails.Status ?? StatusCodes.Status500InternalServerError;

        await httpContext.Response.WriteAsJsonAsync(
            problemDetails,
            options: null,
            contentType: "application/problem+json",
            cancellationToken);

        return true;
    }

    private async Task TryWriteAuditAsync(
        HttpContext httpContext,
        Exception exception,
        ProblemDetails problemDetails,
        CancellationToken cancellationToken)
    {
        var auditOptions = _auditOptions.Value;
        if (!auditOptions.Enabled || !auditOptions.CaptureErrors)
        {
            return;
        }

        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var audit = scope.ServiceProvider.GetRequiredService<IAuditService>();

            var isApp = exception is AppException;
            var status = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
            var category = status is StatusCodes.Status401Unauthorized or StatusCodes.Status403Forbidden
                ? AuditCategories.Security
                : AuditCategories.Error;

            await audit.WriteAsync(
                new AuditEvent
                {
                    Action = isApp ? "Error.AppException" : "Error.Unhandled",
                    Category = category,
                    Outcome = status is StatusCodes.Status401Unauthorized or StatusCodes.Status403Forbidden
                        ? AuditOutcomes.Denied
                        : AuditOutcomes.Failure,
                    EntityType = exception.GetType().Name,
                    HttpMethod = httpContext.Request.Method,
                    RequestPath = httpContext.Request.Path.Value,
                    StatusCode = status,
                    ErrorCode = isApp ? ((AppException)exception).ErrorCode : ErrorCodes.Internal,
                    ExceptionType = exception.GetType().FullName,
                    After = new
                    {
                        message = isApp ? exception.Message : "Beklenmeyen hata",
                        traceId = GetTraceId(httpContext)
                    }
                },
                cancellationToken);
        }
        catch (Exception auditEx)
        {
            _logger.LogDebug(auditEx, "Exception audit yazılamadı.");
        }
    }

    private ProblemDetails HandleApplicationException(
        HttpContext httpContext,
        AppException exception)
    {
        // Beklenen domain hatası: düşük log seviyesi, güvenli mesaj.
        _logger.LogWarning(
            exception,
            "Handled application exception. ErrorCode: {ErrorCode}, "
            + "StatusCode: {StatusCode}, Method: {RequestMethod}, "
            + "Path: {RequestPath}, ExceptionType: {ExceptionType}, "
            + "TraceId: {TraceId}",
            exception.ErrorCode,
            exception.StatusCode,
            httpContext.Request.Method,
            httpContext.Request.Path,
            exception.GetType().Name,
            GetTraceId(httpContext));

        if (_options.LogValidationErrors
            && exception.Errors is { Count: > 0 } validationErrors)
        {
            _logger.LogWarning(
                "Validation failed for {FieldCount} field(s): {Fields}",
                validationErrors.Count,
                string.Join(", ", validationErrors.Keys));
        }

        return ImeceProblemDetailsFactory.Create(
            httpContext,
            exception.StatusCode,
            exception.ErrorCode,
            ImeceProblemDetailsFactory.TitleFor(exception.ErrorCode),
            exception.Message,
            exception.Errors);
    }

    private ProblemDetails HandleUnexpectedException(
        HttpContext httpContext,
        Exception exception)
    {
        // Beklenmeyen hata: yüksek log seviyesi, jenerik kullanıcı mesajı.
        _logger.LogError(
            exception,
            "Unhandled exception occurred. StatusCode: {StatusCode}, "
            + "Method: {RequestMethod}, Path: {RequestPath}, "
            + "ExceptionType: {ExceptionType}, TraceId: {TraceId}",
            StatusCodes.Status500InternalServerError,
            httpContext.Request.Method,
            httpContext.Request.Path,
            exception.GetType().Name,
            GetTraceId(httpContext));

        var problemDetails = ImeceProblemDetailsFactory.Create(
            httpContext,
            StatusCodes.Status500InternalServerError,
            ErrorCodes.Internal,
            ImeceProblemDetailsFactory.TitleFor(ErrorCodes.Internal),
            "Beklenmeyen bir hata oluştu.");

        if (_options.IncludeExceptionDetailsInDevelopment
            && _environment.IsDevelopment())
        {
            problemDetails.Extensions["exception"] = new
            {
                type = exception.GetType().FullName,
                message = exception.Message,
                stackTrace = exception.StackTrace
            };
        }

        return problemDetails;
    }

    private static string GetTraceId(HttpContext httpContext) =>
        Activity.Current?.Id ?? httpContext.TraceIdentifier;
}
