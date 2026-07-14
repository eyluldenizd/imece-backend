using Application.DTOs;

namespace Application.Validators;

public static class AnnouncementValidator
{
    private const int TitleMaxLength = 200;
    private const int ContentMaxLength = 4000;

    public static void ValidateCreate(CreateAnnouncementDto dto)
    {
        var errors = new Dictionary<string, List<string>>();

        ValidateCommonFields(
            dto.Title,
            dto.Content,
            dto.AuthorUserId,
            dto.PublishStart,
            dto.PublishEnd,
            dto.CoverImageUrl,
            errors);

        ThrowIfInvalid(errors);
    }

    public static void ValidateUpdate(UpdateAnnouncementDto dto)
    {
        var errors = new Dictionary<string, List<string>>();

        if (dto.AnnouncementId <= 0)
        {
            AddError(errors, nameof(dto.AnnouncementId), "AnnouncementId geçerli bir değer olmalıdır.");
        }

        ValidateCommonFields(
            dto.Title,
            dto.Content,
            dto.AuthorUserId,
            dto.PublishStart,
            dto.PublishEnd,
            dto.CoverImageUrl,
            errors);

        ThrowIfInvalid(errors);
    }

    private static void ValidateCommonFields(
        string? title,
        string? content,
        int authorUserId,
        DateTime publishStart,
        DateTime? publishEnd,
        string? coverImageUrl,
        Dictionary<string, List<string>> errors)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            AddError(errors, "Title", "Başlık boş olamaz.");
        }
        else if (title.Length > TitleMaxLength)
        {
            AddError(errors, "Title", $"Başlık en fazla {TitleMaxLength} karakter olabilir.");
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            AddError(errors, "Content", "İçerik boş olamaz.");
        }
        else if (content.Length > ContentMaxLength)
        {
            AddError(errors, "Content", $"İçerik en fazla {ContentMaxLength} karakter olabilir.");
        }

        if (authorUserId <= 0)
        {
            AddError(errors, "AuthorUserId", "Geçerli bir yazar (AuthorUserId) belirtilmelidir.");
        }

        if (publishEnd.HasValue && publishEnd.Value <= publishStart)
        {
            AddError(errors, "PublishEnd", "Yayın bitiş tarihi, başlangıç tarihinden sonra olmalıdır.");
        }

        if (!string.IsNullOrWhiteSpace(coverImageUrl) &&
            !Uri.TryCreate(coverImageUrl, UriKind.Absolute, out _))
        {
            AddError(errors, "CoverImageUrl", "Kapak görseli geçerli bir URL olmalıdır.");
        }
    }

    private static void AddError(Dictionary<string, List<string>> errors, string key, string message)
    {
        if (!errors.TryGetValue(key, out var list))
        {
            list = new List<string>();
            errors[key] = list;
        }

        list.Add(message);
    }

    private static void ThrowIfInvalid(Dictionary<string, List<string>> errors)
    {
        if (errors.Count == 0)
        {
            return;
        }

        var finalErrors = errors.ToDictionary(x => x.Key, x => x.Value.ToArray());
        throw new Exceptions.ValidationException(finalErrors);
    }
}