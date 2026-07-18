namespace ImeceWebAPI.Options;

/// <summary>
/// ClientApplication çözümleme yapılandırması. İstemci uygulaması güvenilmeyen
/// ham header değerinden DEĞİL; yalnızca allow-list ile doğrulanmış bir eşlemeden
/// belirlenir. Bilinmeyen/eksik değerler <see cref="DefaultForRequests"/>'e düşer;
/// HTTP bağlamı yoksa <see cref="SystemValue"/> kullanılır.
/// </summary>
public sealed class ClientApplicationOptions
{
    public const string SectionName = "ClientApplication";

    /// <summary>İstemcinin uygulama kimliğini bildirdiği header adı.</summary>
    public string HeaderName { get; set; } = "X-Client-Application";

    /// <summary>Kabul edilen (doğrulanmış) değerler.</summary>
    public string[] AllowedValues { get; set; } = ["Web", "AdminPanel"];

    /// <summary>Header yok/geçersizse kullanıcı istekleri için varsayılan.</summary>
    public string DefaultForRequests { get; set; } = "Web";

    /// <summary>HTTP bağlamı olmayan (arka plan) işlemler için değer.</summary>
    public string SystemValue { get; set; } = "System";
}
