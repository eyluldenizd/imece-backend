using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace ImeceWebAPI.Services.Storage;

public static partial class FileNameSanitizer
{
    private static readonly HashSet<string> AllowedExtensions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".webp", ".gif",
            ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".mp4"
        };

    private static readonly HashSet<string> BlockedExtensions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ".exe", ".js", ".html", ".svg"
        };

    public static string SanitizeCompanySlug(string? companyName, int companyId)
    {
        if (string.IsNullOrWhiteSpace(companyName))
        {
            return $"company-{companyId}";
        }

        var normalized = companyName.Trim().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);

        foreach (var character in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            if (char.IsAsciiLetterOrDigit(character))
            {
                builder.Append(char.ToLowerInvariant(character));
                continue;
            }

            if (character is ' ' or '-' or '_')
            {
                builder.Append('-');
            }
        }

        var slug = CollapseHyphensRegex().Replace(builder.ToString(), "-").Trim('-');
        return string.IsNullOrWhiteSpace(slug) ? $"company-{companyId}" : slug;
    }

    public static string GetSafeExtension(string originalFileName)
    {
        var extension = Path.GetExtension(originalFileName).ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(extension))
        {
            throw new InvalidOperationException("Dosya uzantısı bulunamadı.");
        }

        if (BlockedExtensions.Contains(extension))
        {
            throw new InvalidOperationException($"Güvenlik nedeniyle {extension} uzantılı dosyalar kabul edilmiyor.");
        }

        if (!AllowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException($"Desteklenmeyen dosya uzantısı: {extension}");
        }

        return extension;
    }

    public static string CreateStoredFileName(string extension) =>
        $"{Guid.NewGuid():N}{extension}";

    [GeneratedRegex("-{2,}")]
    private static partial Regex CollapseHyphensRegex();
}
