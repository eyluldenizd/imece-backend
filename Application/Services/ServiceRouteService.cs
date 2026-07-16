using Application.DTOs;
using Infrastructure.Entities;
using Infrastructure.Repositories;

namespace Application.Services;

public sealed class ServiceRouteService
{
    private readonly ServiceRouteRepository _repository;

    public ServiceRouteService(
        ServiceRouteRepository repository)
    {
        _repository = repository;
    }


    public async Task<List<ServiceRouteDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var entities = await _repository.GetAllAsync(
            cancellationToken);

        return entities.Select(x => new ServiceRouteDto
        {
            ServiceRouteId = x.ServiceRouteId,
            RouteName = x.RouteName,
            DepartureLocation = x.DepartureLocation,
            ArrivalLocation = x.ArrivalLocation,
            RouteDescription = x.RouteDescription,
            DepartureTime = x.DepartureTime,
            ArrivalTime = x.ArrivalTime,
            IsActive = x.IsActive,
            DisplayOrder = x.DisplayOrder,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt
        }).ToList();
    }


    public async Task<ServiceRouteDto?> GetByIdAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(
            id,
            cancellationToken);

        if (entity == null)
        {
            return null;
        }

        return new ServiceRouteDto
        {
            ServiceRouteId = entity.ServiceRouteId,
            RouteName = entity.RouteName,
            DepartureLocation = entity.DepartureLocation,
            ArrivalLocation = entity.ArrivalLocation,
            RouteDescription = entity.RouteDescription,
            DepartureTime = entity.DepartureTime,
            ArrivalTime = entity.ArrivalTime,
            IsActive = entity.IsActive,
            DisplayOrder = entity.DisplayOrder,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }


    public Task CreateAsync(
        ServiceRouteDto dto,
        CancellationToken cancellationToken = default)
    {
        var entity = new ServiceRoutes
        {
            RouteName = dto.RouteName,
            DepartureLocation = dto.DepartureLocation,
            ArrivalLocation = dto.ArrivalLocation,
            RouteDescription = dto.RouteDescription,
            DepartureTime = dto.DepartureTime,
            ArrivalTime = dto.ArrivalTime,
            IsActive = dto.IsActive,
            DisplayOrder = dto.DisplayOrder
        };

        return _repository.CreateAsync(
            entity,
            cancellationToken);
    }


    public Task UpdateAsync(
        ServiceRouteDto dto,
        CancellationToken cancellationToken = default)
    {
        var entity = new ServiceRoutes
        {
            ServiceRouteId = dto.ServiceRouteId,
            RouteName = dto.RouteName,
            DepartureLocation = dto.DepartureLocation,
            ArrivalLocation = dto.ArrivalLocation,
            RouteDescription = dto.RouteDescription,
            DepartureTime = dto.DepartureTime,
            ArrivalTime = dto.ArrivalTime,
            IsActive = dto.IsActive,
            DisplayOrder = dto.DisplayOrder
        };

        return _repository.UpdateAsync(
            entity,
            cancellationToken);
    }


    public Task DeleteAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        return _repository.DeleteAsync(
            id,
            cancellationToken);
    }
}