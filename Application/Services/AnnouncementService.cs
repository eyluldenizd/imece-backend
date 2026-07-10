using Application.DTOs;
using Infrastructure.Repositories;

namespace Application.Services;

public class AnnouncementService
{
    private readonly AnnouncementRepository _announcementRepository;

    public AnnouncementService(
        AnnouncementRepository announcementRepository)
    {
        _announcementRepository = announcementRepository;
    }

    public async Task<List<AnnouncementDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var announcements =
            await _announcementRepository.GetAllAsync(cancellationToken);

        return announcements.Select(x => new AnnouncementDto
        {
            AnnouncementId = x.AnnouncementId,
            Title = x.Title,
            Content = x.Content,
            CoverImageUrl = x.CoverImageUrl,
            AuthorUserId = x.AuthorUserId,
            IsPinned = x.IsPinned,
            PublishStart = x.PublishStart,
            PublishEnd = x.PublishEnd,
            ViewCount = x.ViewCount,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt
        }).ToList();
    }
}