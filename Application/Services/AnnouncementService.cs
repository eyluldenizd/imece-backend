using Application.DTOs;
using Core.Common;
using Infrastructure.Entities;
using Infrastructure.Repositories;

namespace Application.Services;

public sealed class AnnouncementService
{
    private readonly AnnouncementRepository _announcementRepository;

    public AnnouncementService(
        AnnouncementRepository announcementRepository)
    {
        _announcementRepository = announcementRepository;
    }

    public async Task<ServiceResult<IReadOnlyList<AnnouncementDto>>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var announcements =
            await _announcementRepository.GetAllAsync(
                cancellationToken);

        var response = announcements
            .Select(ToDto)
            .ToList();

        return ServiceResult<IReadOnlyList<AnnouncementDto>>
            .Success(response);
    }

    public async Task<ServiceResult<IReadOnlyList<AnnouncementDto>>> GetPublishedAsync(
        CancellationToken cancellationToken = default)
    {
        var announcements =
            await _announcementRepository.GetPublishedAsync(
                cancellationToken);

        var response = announcements
            .Select(ToDto)
            .ToList();

        return ServiceResult<IReadOnlyList<AnnouncementDto>>
            .Success(response);
    }

    public async Task<ServiceResult<AnnouncementDto>> GetByIdAsync(
        IdRequest request,
        CancellationToken cancellationToken = default)
    {
        var announcement =
            await _announcementRepository.GetByIdAsync(
                request.Id,
                cancellationToken);

        if (announcement is null)
        {
            return ServiceResult<AnnouncementDto>.NotFound(
                $"ID değeri {request.Id} olan duyuru bulunamadı.");
        }

        return ServiceResult<AnnouncementDto>.Success(
            ToDto(announcement));
    }

    public async Task<ServiceResult<long>> CreateAsync(
        CreateAnnouncementDto request,
        CancellationToken cancellationToken = default)
    {
        var announcement = CreateEntity(request);

        var announcementId =
            await _announcementRepository.CreateAsync(
                announcement,
                cancellationToken);

        return ServiceResult<long>.Created(
            announcementId);
    }

    public async Task<ServiceResult> UpdateAsync(
        UpdateAnnouncementDto request,
        CancellationToken cancellationToken = default)
    {
        var announcement =
            await _announcementRepository.GetByIdAsync(
                request.AnnouncementId,
                cancellationToken);

        if (announcement is null)
        {
            return ServiceResult.NotFound(
                $"ID değeri {request.AnnouncementId} olan duyuru bulunamadı.");
        }

        ApplyChanges(
            announcement,
            request);

        var rowsAffected =
            await _announcementRepository.UpdateAsync(
                announcement,
                cancellationToken);

        if (rowsAffected == 0)
        {
            return ServiceResult.Conflict(
                "Duyuru güncellenemedi. Kayıt başka bir işlem tarafından değiştirilmiş olabilir.");
        }

        return ServiceResult.NoContent();
    }

    public async Task<ServiceResult> DeleteAsync(
        IdRequest request,
        CancellationToken cancellationToken = default)
    {
        var rowsAffected =
            await _announcementRepository.DeleteAsync(
                request.Id,
                cancellationToken);

        if (rowsAffected == 0)
        {
            return ServiceResult.NotFound(
                $"ID değeri {request.Id} olan duyuru bulunamadı.");
        }

        return ServiceResult.NoContent();
    }

    private static Announcements CreateEntity(
        CreateAnnouncementDto request)
    {
        return new Announcements
        {
            Title = request.Title,
            Content = request.Content,
            CoverImageUrl = request.CoverImageUrl,
            AuthorUserId = request.AuthorUserId,
            IsPinned = request.IsPinned,
            PublishStart = request.PublishStart,
            PublishEnd = request.PublishEnd
        };
    }

    private static void ApplyChanges(
        Announcements announcement,
        UpdateAnnouncementDto request)
    {
        announcement.Title = request.Title;
        announcement.Content = request.Content;
        announcement.CoverImageUrl = request.CoverImageUrl;
        announcement.AuthorUserId = request.AuthorUserId;
        announcement.IsPinned = request.IsPinned;
        announcement.PublishStart = request.PublishStart;
        announcement.PublishEnd = request.PublishEnd;
    }

    private static AnnouncementDto ToDto(
        Announcements announcement)
    {
        return new AnnouncementDto
        {
            AnnouncementId = announcement.AnnouncementId,
            Title = announcement.Title,
            Content = announcement.Content,
            CoverImageUrl = announcement.CoverImageUrl,
            AuthorUserId = announcement.AuthorUserId,
            IsPinned = announcement.IsPinned,
            PublishStart = announcement.PublishStart,
            PublishEnd = announcement.PublishEnd,
            ViewCount = announcement.ViewCount,
            CreatedAt = announcement.CreatedAt,
            UpdatedAt = announcement.UpdatedAt
        };
    }
}