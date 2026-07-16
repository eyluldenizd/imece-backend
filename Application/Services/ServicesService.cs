using Application.DTOs;
using Core.Common;
using Core.Entities;
using Infrastructure.Repositories;

namespace Application.Services;

public sealed class ServicesService
{
    private readonly ServicesRepository _servicesRepository;

    public ServicesService(ServicesRepository servicesRepository)
    {
        _servicesRepository = servicesRepository;
    }

    public async Task<ServiceResult<IReadOnlyList<ServiceDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var list = await _servicesRepository.GetAllAsync(cancellationToken);
        return ServiceResult<IReadOnlyList<ServiceDto>>.Success(list.Select(ToDto).ToList());
    }

    public async Task<ServiceResult<IReadOnlyList<ServiceDto>>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        var list = await _servicesRepository.GetActiveAsync(cancellationToken);
        return ServiceResult<IReadOnlyList<ServiceDto>>.Success(list.Select(ToDto).ToList());
    }

    public async Task<ServiceResult<ServiceDto>> GetByIdAsync(IdRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _servicesRepository.GetByIdAsync(request.Id, cancellationToken);
        if (entity is null)
            return ServiceResult<ServiceDto>.NotFound("Hizmet bulunamadı.");

        return ServiceResult<ServiceDto>.Success(ToDto(entity));
    }

    public async Task<ServiceResult<long>> CreateAsync(CreateServiceDto request, CancellationToken cancellationToken = default)
    {
        var entity = new Services
        {
            Name = request.Name,
            Description = request.Description,
            Icon = request.Icon,
            IsActive = true
        };

        var id = await _servicesRepository.CreateAsync(entity, cancellationToken);
        return ServiceResult<long>.Created(id);
    }

    public async Task<ServiceResult> UpdateAsync(UpdateServiceDto request, CancellationToken cancellationToken = default)
    {
        var entity = await _servicesRepository.GetByIdAsync(request.ServiceId, cancellationToken);
        if (entity is null)
            return ServiceResult.NotFound("Hizmet bulunamadı.");

        entity.Name = request.Name;
        entity.Description = request.Description;
        entity.Icon = request.Icon;
        entity.IsActive = request.IsActive;

        await _servicesRepository.UpdateAsync(entity, cancellationToken);
        return ServiceResult.NoContent();
    }

    public async Task<ServiceResult> DeleteAsync(IdRequest request, CancellationToken cancellationToken = default)
    {
        await _servicesRepository.DeleteAsync(request.Id, cancellationToken);
        return ServiceResult.NoContent();
    }

    private static ServiceDto ToDto(Services entity) => new()
    {
        ServiceId = entity.ServiceId,
        Name = entity.Name,
        Description = entity.Description,
        Icon = entity.Icon,
        IsActive = entity.IsActive
    };
}
