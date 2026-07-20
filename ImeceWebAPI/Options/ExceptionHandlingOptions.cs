namespace ImeceWebAPI.Options;

/// <summary>
/// Merkezî exception handling ve hata response davranışını kontrol eden
/// strongly typed ayarlar. <c>appsettings.json</c> içindeki
/// <see cref="SectionName"/> bölümünden bind edilir ve startup sırasında
/// doğrulanır.
/// </summary>
public sealed class ExceptionHandlingOptions
{
    public const string SectionName = "ExceptionHandling";

    /// <summary>
    /// Yalnızca Development ortamında ve açıkça etkinleştirildiğinde hata
    /// response'una exception tipi/mesajı gibi teşhis bilgisi ekler.
    /// Production'da bu ayar dikkate alınmaz. Stack trace hiçbir durumda
    /// response'a eklenmez.
    /// </summary>
    public bool IncludeExceptionDetailsInDevelopment { get; set; }

    /// <summary>
    /// Hata response'larına <c>traceId</c> alanının eklenip eklenmeyeceği.
    /// </summary>
    public bool IncludeTraceId { get; set; } = true;

    /// <summary>
    /// Alan bazlı doğrulama hatalarının log'a yazılıp yazılmayacağı.
    /// Varsayılan kapalıdır; hassas alan değerleri loglanmaz.
    /// </summary>
    public bool LogValidationErrors { get; set; }
}
