using Core.Common;

namespace ImeceWebAPI.Infrastructure;

/// <summary>
/// <see cref="IRequestCancellation"/>'ın HTTP implementasyonu. Token her
/// erişimde güncel <c>HttpContext.RequestAborted</c>'tan okunur; böylece
/// gerçek istek yaşam döngüsüyle senkron ("yaşayan") kalır. İstek bağlamı
/// yoksa (ör. hosted service) <see cref="CancellationToken.None"/> döner.
/// </summary>
public sealed class HttpRequestCancellation : IRequestCancellation
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpRequestCancellation(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public CancellationToken Token =>
        _httpContextAccessor.HttpContext?.RequestAborted
        ?? CancellationToken.None;
}
