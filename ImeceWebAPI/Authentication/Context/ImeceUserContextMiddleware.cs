using Core.Authentication;
using Core.Authorization;

namespace ImeceWebAPI.Authentication.Context;

/// <summary>
/// Authentication tamamlandıktan sonra çalışır; doğrulanmış dış kimliği
/// uygulama kullanıcısına çevirip istek scope'undaki
/// <see cref="ImeceUserContext"/>'e yazar. Authorization'dan önce çalışması
/// gerekir; policy'ler çözümlenmiş kullanıcıyı kullanır.
/// </summary>
public sealed class ImeceUserContextMiddleware
{
    private readonly RequestDelegate _next;

    public ImeceUserContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext httpContext,
        IExternalIdentityAccessor externalIdentityAccessor,
        IApplicationUserResolver applicationUserResolver,
        ImeceUserContext userContext)
    {
        var externalIdentity = externalIdentityAccessor.GetExternalIdentity();

        if (externalIdentity is not null)
        {
            var applicationUser = await applicationUserResolver.ResolveAsync(
                externalIdentity,
                httpContext.RequestAborted);

            userContext.Set(applicationUser);
        }

        await _next(httpContext);
    }
}
