using Core.Authorization;

namespace ImeceWebAPI.Authentication.Context;

/// <summary>
/// İstek başına (scoped) çözümlenmiş uygulama kullanıcısını tutar. Kullanıcı,
/// <c>ImeceUserContextMiddleware</c> tarafından bir kez çözülüp buraya yazılır;
/// <see cref="CurrentUser"/> ve <see cref="CompanyContext"/> senkron olarak
/// bu değeri okur. Böylece dizin çözümlemesi istek başına tek sefer yapılır.
/// </summary>
public sealed class ImeceUserContext
{
    public ApplicationUser? User { get; private set; }

    public bool IsResolved => User is not null;

    public void Set(ApplicationUser user) => User = user;
}
