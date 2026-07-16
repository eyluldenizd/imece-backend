namespace Core.Common.Messages;

public static class MediaFileMessages
{
    public const string FileRequired = "Yüklenecek dosya zorunludur.";
    public const string FileEmpty = "Boş dosya yüklenemez.";
    public const string FileTooLarge = "Dosya boyutu izin verilen 25 MB sınırını aşıyor.";
    public const string UnsupportedExtension = "Dosya uzantısı desteklenmiyor.";
    public const string MediaTypeInvalid = "Medya türü Photo, Video veya Document olmalıdır.";
    public const string FileMediaTypeMismatch = "Dosya uzantısı seçilen medya türüyle uyumlu değildir.";
    public const string FolderInactive = "Pasif bir klasöre dosya yüklenemez.";
    public const string FolderCompanyMismatch = "Medya dosyası ile hedef klasör aynı şirkete ait olmalıdır.";
    public const string FolderMediaTypeMismatch = "Medya dosyasının türü hedef klasör türüyle uyumlu değildir.";
    public const string SortOrderInvalid = "Sıralama değeri negatif olamaz.";
    public const string DurationInvalid = "Video süresi negatif olamaz.";
    public const string DateRangeInvalid = "Doküman geçerlilik bitiş tarihi, başlangıç tarihinden önce olamaz.";

    public static string FolderNotFound(long folderId) =>
        $"ID değeri {folderId} olan medya klasörü bulunamadı.";
}
