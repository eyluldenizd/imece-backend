using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Core.Auditing;
using Infrastructure.Database.Options;
using Microsoft.Extensions.Options;

namespace ImeceWebAPI.Auditing;

/// <summary>
/// Her API isteğini otomatik Request audit'e yazar.
/// Controller/service'e dokunmaz; pipeline seviyesinde yaşar.
/// </summary>
public sealed class RequestAuditMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IOptions<AuditOptions> _options;
    private readonly ILogger<RequestAuditMiddleware> _logger;

    public RequestAuditMiddleware(
        RequestDelegate next,
        IOptions<AuditOptions> options,
        ILogger<RequestAuditMiddleware> logger)
    {
        _next = next;
        _options = options;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IAuditService auditService)
    {
        var options = _options.Value;
        if (!options.Enabled || !options.CaptureHttpRequests || IsExcluded(context.Request.Path, options))
        {
            await _next(context);
            return;
        }

        var sw = Stopwatch.StartNew();
        object? requestBody = null;

        if (ShouldCaptureBody(context.Request.Method))
        {
            requestBody = await ReadBodyPreviewAsync(context.Request, options.MaxBodyBytes);
        }

        try
        {
            await _next(context);
        }
        finally
        {
            sw.Stop();
            try
            {
                var status = context.Response.StatusCode;
                var outcome = status >= 200 && status <= 399
                    ? AuditOutcomes.Success
                    : status is 401 or 403
                        ? AuditOutcomes.Denied
                        : AuditOutcomes.Failure;

                var category = status is 401 or 403
                    ? AuditCategories.Security
                    : AuditCategories.Request;

                await auditService.WriteAsync(
                    new AuditEvent
                    {
                        Action = $"Http.{context.Request.Method}",
                        Category = category,
                        Outcome = outcome,
                        HttpMethod = context.Request.Method,
                        RequestPath = context.Request.Path.Value,
                        StatusCode = status,
                        DurationMs = sw.ElapsedMilliseconds,
                        RequestBody = requestBody,
                        After = new
                        {
                            query = context.Request.QueryString.HasValue
                                ? context.Request.QueryString.Value
                                : null,
                            status
                        }
                    },
                    CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Request audit yazılamadı: {Path}", context.Request.Path);
            }
        }
    }

    private static bool IsExcluded(PathString path, AuditOptions options)
    {
        var value = path.Value ?? string.Empty;
        return options.ExcludedPathPrefixes.Any(prefix =>
            value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
    }

    private static bool ShouldCaptureBody(string method) =>
        HttpMethods.IsPost(method)
        || HttpMethods.IsPut(method)
        || HttpMethods.IsPatch(method);

    private static async Task<object?> ReadBodyPreviewAsync(HttpRequest request, int maxBytes)
    {
        if (!request.Body.CanRead)
        {
            return null;
        }

        request.EnableBuffering();
        request.Body.Position = 0;

        using var reader = new StreamReader(
            request.Body,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            bufferSize: 1024,
            leaveOpen: true);

        var buffer = new char[Math.Min(maxBytes, 8192)];
        var read = await reader.ReadBlockAsync(buffer, 0, buffer.Length);
        request.Body.Position = 0;

        if (read <= 0)
        {
            return null;
        }

        var text = new string(buffer, 0, read);
        if (read >= maxBytes)
        {
            text += "…(truncated)";
        }

        try
        {
            using var doc = JsonDocument.Parse(text);
            return JsonSerializer.Deserialize<Dictionary<string, object?>>(text);
        }
        catch
        {
            return text;
        }
    }
}
