using Application.DTOs;
using Application.Exceptions;
using Application.Validators;
using Infrastructure.Entities;
using Infrastructure.Repositories;

namespace Application.Services;

public class AnnouncementService
{
    private readonly AnnouncementRepository _announcementRepository;

    public AnnouncementService(AnnouncementRepository announcementRepository)
    {
        _announcementRepository = announcementRepository;
    }

    public async Task<List<AnnouncementDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var announcements = await _announcementRepository.GetAllAsync(cancellationToken);
        return announcements.Select(ToDto).ToList();
    }

    public async Task<List<AnnouncementDto>> GetPublishedAsync(
        CancellationToken cancellationToken = default)
    {
        var announcements = await _announcementRepository.GetPublishedAsync(cancellationToken);
        return announcements.Select(ToDto).ToList();
    }

    public async Task<AnnouncementDto> GetByIdAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        var announcement = await _announcementRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Announcements), id);

        return ToDto(announcement);
    }

    public async Task<long> CreateAsync(
        CreateAnnouncementDto dto,
        CancellationToken cancellationToken = default)
    {
        // Validation, repository çağrılmadan önce burada yapılır.
        AnnouncementValidator.ValidateCreate(dto);

        var announcement = new Announcements
        {
            Title = dto.Title,
            Content = dto.Content,
            CoverImageUrl = dto.CoverImageUrl,
            AuthorUserId = dto.AuthorUserId,
            IsPinned = dto.IsPinned,
            PublishStart = dto.PublishStart,
            PublishEnd = dto.PublishEnd
        };

        return await _announcementRepository.CreateAsync(announcement, cancellationToken);
    }

    public async Task UpdateAsync(
        UpdateAnnouncementDto dto,
        CancellationToken cancellationToken = default)
    {
        AnnouncementValidator.ValidateUpdate(dto);

        var existing = await _announcementRepository.GetByIdAsync(dto.AnnouncementId, cancellationToken)
            ?? throw new NotFoundException(nameof(Announcements), dto.AnnouncementId);

        existing.Title = dto.Title;
        existing.Content = dto.Content;
        existing.CoverImageUrl = dto.CoverImageUrl;
        existing.AuthorUserId = dto.AuthorUserId;
        existing.IsPinned = dto.IsPinned;
        existing.PublishStart = dto.PublishStart;
        existing.PublishEnd = dto.PublishEnd;

        var rowsAffected = await _announcementRepository.UpdateAsync(existing, cancellationToken);
        if (rowsAffected == 0)
        {
            throw new NotFoundException(nameof(Announcements), dto.AnnouncementId);
        }
    }

    public async Task DeleteAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        var rowsAffected = await _announcementRepository.DeleteAsync(id, cancellationToken);
        if (rowsAffected == 0)
        {
            throw new NotFoundException(nameof(Announcements), id);
        }
    }

    private static AnnouncementDto ToDto(Announcements x) => new()
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
    };
}