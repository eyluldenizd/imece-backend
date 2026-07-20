namespace Application.Common.Codes;

/// <summary>
/// Generates unique business codes from optional user input or entity name.
/// Collision suffixes: BASE, BASE-2, BASE-3, …
/// </summary>
public interface IEntityCodeGenerator
{
    Task<string> AllocateAsync(
        string? preferredCode,
        string nameFallback,
        string emptyPrefix,
        Func<string, CancellationToken, Task<bool>> existsAsync,
        int maxLength = 64,
        CancellationToken cancellationToken = default);
}

public sealed class EntityCodeGenerator : IEntityCodeGenerator
{
    public async Task<string> AllocateAsync(
        string? preferredCode,
        string nameFallback,
        string emptyPrefix,
        Func<string, CancellationToken, Task<bool>> existsAsync,
        int maxLength = 64,
        CancellationToken cancellationToken = default)
    {
        var baseCode = Normalize(
            !string.IsNullOrWhiteSpace(preferredCode) ? preferredCode! : nameFallback,
            maxLength);

        if (string.IsNullOrWhiteSpace(baseCode))
        {
            baseCode = Normalize(
                $"{emptyPrefix}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}",
                maxLength);
        }

        var candidate = baseCode;
        var suffix = 2;
        while (await existsAsync(candidate, cancellationToken).ConfigureAwait(false))
        {
            var suffixText = $"-{suffix}";
            var trimLen = Math.Max(1, maxLength - suffixText.Length);
            candidate = $"{baseCode[..Math.Min(baseCode.Length, trimLen)]}{suffixText}";
            suffix++;
            if (suffix > 5000)
            {
                throw new InvalidOperationException("Benzersiz kod üretilemedi.");
            }
        }

        return candidate;
    }

    public static string Normalize(string value, int maxLength = 64)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var map = new Dictionary<char, string>
        {
            ['ç'] = "C",
            ['Ç'] = "C",
            ['ğ'] = "G",
            ['Ğ'] = "G",
            ['ı'] = "I",
            ['İ'] = "I",
            ['ö'] = "O",
            ['Ö'] = "O",
            ['ş'] = "S",
            ['Ş'] = "S",
            ['ü'] = "U",
            ['Ü'] = "U",
        };

        var chars = new List<char>(value.Length);
        foreach (var ch in value.Trim())
        {
            if (map.TryGetValue(ch, out var mapped))
            {
                chars.AddRange(mapped);
                continue;
            }

            if (char.IsLetterOrDigit(ch))
            {
                chars.Add(char.ToUpperInvariant(ch));
                continue;
            }

            if (ch is '-' or '_' or ' ')
            {
                if (chars.Count > 0 && chars[^1] != '-')
                {
                    chars.Add('-');
                }
            }
        }

        while (chars.Count > 0 && chars[^1] == '-')
        {
            chars.RemoveAt(chars.Count - 1);
        }

        var result = new string(chars.ToArray());
        if (result.Length > maxLength)
        {
            result = result[..maxLength].TrimEnd('-');
        }

        return result;
    }
}
