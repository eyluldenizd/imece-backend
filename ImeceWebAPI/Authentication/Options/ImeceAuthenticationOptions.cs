namespace ImeceWebAPI.Authentication.Options;

/// <summary>
/// <c>Authentication</c> configuration bölümü. <see cref="Mode"/> ile aktif
/// provider seçilir; provider'a özgü ayarlar alt bölümlerde tutulur.
/// </summary>
public sealed class ImeceAuthenticationOptions
{
    public const string SectionName = "Authentication";

    public AuthenticationMode Mode { get; set; } = AuthenticationMode.Development;

    public DevelopmentAuthenticationSettings Development { get; set; } = new();

    public JwtAuthenticationSettings Jwt { get; set; } = new();
}

/// <summary>
/// LocalJwt (username/password) authentication ayarları. SigningKey yalnızca
/// configuration'dan okunur; kaynak kodda sabitlenmez.
/// </summary>
public sealed class JwtAuthenticationSettings
{
    public string Issuer { get; set; } = "imece-webapi";

    public string Audience { get; set; } = "imece-admin";

    public string SigningKey { get; set; } = string.Empty;

    public int AccessTokenMinutes { get; set; } = 480;
}

/// <summary>
/// Development authentication provider ayarları. Yalnızca Development
/// ortamında anlamlıdır ve production kodu bu tipe bağlı değildir.
/// </summary>
public sealed class DevelopmentAuthenticationSettings
{
    /// <summary>Header ile seçim yapılmadığında kullanılacak profil.</summary>
    public string DefaultProfile { get; set; } = "registered-user";

    /// <summary>Development'ta profil seçimi için okunacak header adı.</summary>
    public string UserHeaderName { get; set; } = "X-Imece-Dev-User";

    /// <summary>Header ile profil seçimine izin verilsin mi (yalnızca Development).</summary>
    public bool AllowHeaderSelection { get; set; } = true;

    /// <summary>Profil anahtarı → simüle edilen dış kimlik.</summary>
    public Dictionary<string, DevelopmentUserProfile> Profiles { get; set; } =
        new(StringComparer.OrdinalIgnoreCase);
}

/// <summary>
/// Development'ta bir authentication provider'ın (ör. AD) döndüreceği ham
/// kimliği simüle eder. Uygulama profili (şirket/rol) bilgisini içermez; o
/// bilgi dizin servisinden gelir.
/// </summary>
public sealed class DevelopmentUserProfile
{
    public string? IdentityProvider { get; set; }

    public string ExternalId { get; set; } = string.Empty;

    public string? Username { get; set; }

    public string? Email { get; set; }

    public string? DisplayName { get; set; }

    public string? DomainOrTenant { get; set; }
}
