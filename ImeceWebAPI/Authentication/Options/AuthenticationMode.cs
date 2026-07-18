namespace ImeceWebAPI.Authentication.Options;

/// <summary>
/// Aktif authentication provider modu. Yalnızca <see cref="Development"/> bu
/// turda tamamdır; <see cref="Negotiate"/> ve <see cref="EntraId"/> genişletme
/// noktalarıdır ve configuration/infrastructure adapter eklendiğinde devreye
/// alınır (business katmanı değişmeden).
/// </summary>
public enum AuthenticationMode
{
    Development = 0,
    Negotiate = 1,
    EntraId = 2
}
