using System.Diagnostics;
using Core.Auditing;
using ImeceWebAPI.Options;
using Microsoft.Extensions.Options;

namespace ImeceWebAPI.Authentication.Context;

/// <summary>
/// <see cref="IAuditRequestContext"/>'in HTTP implementasyonu. TraceId, IP ve
/// UserAgent güncel <c>HttpContext</c>'ten okunur. ClientApplication güvenilmeyen
/// header'a KÖRÜ KÖRÜNE güvenmez; yalnızca allow-list ile doğrulanmış değer kabul
/// edilir, aksi hâlde güvenli varsayılana düşülür.
/// </summary>
public sealed class HttpAuditRequestContext : IAuditRequestContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ClientApplicationOptions _options;

    public HttpAuditRequestContext(
        IHttpContextAccessor httpContextAccessor,
        IOptions<ClientApplicationOptions> options)
    {
        _httpContextAccessor = httpContextAccessor;
        _options = options.Value;
    }

    public string? TraceId =>
        Activity.Current?.Id ?? _httpContextAccessor.HttpContext?.TraceIdentifier;

    public string? IpAddress =>
        _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

    public string? UserAgent
    {
        get
        {
            var userAgent = _httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString();
            if (string.IsNullOrWhiteSpace(userAgent))
            {
                return null;
            }

            return userAgent.Length > 512 ? userAgent[..512] : userAgent;
        }
    }

    public string ClientApplication
    {
        get
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext is null)
            {
                return _options.SystemValue;
            }

            var declared = httpContext.Request.Headers[_options.HeaderName].ToString();
            var match = _options.AllowedValues.FirstOrDefault(
                allowed => string.Equals(allowed, declared, StringComparison.OrdinalIgnoreCase));

            return match ?? _options.DefaultForRequests;
        }
    }
}
