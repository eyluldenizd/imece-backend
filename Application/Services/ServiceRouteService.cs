using Application.DTOs;
using Core.Common;
using Infrastructure.Entities;
using Infrastructure.Repositories;

namespace Application.Services;
public sealed class ServiceRouteService(ServiceRouteRepository repository)
{
 public async Task<ServiceResult<IReadOnlyList<ServiceRouteDto>>> GetAllAsync(CancellationToken t=default){var x=await repository.GetAllAsync(t);return ServiceResult<IReadOnlyList<ServiceRouteDto>>.Success(x.Select(ToDto).ToList());}
 public async Task<ServiceResult<ServiceRouteDto>> GetByIdAsync(IdRequest r,CancellationToken t=default){var x=await repository.GetByIdAsync(r.Id,t);return x is null?ServiceResult<ServiceRouteDto>.NotFound("Servis güzergâhı bulunamadı."):ServiceResult<ServiceRouteDto>.Success(ToDto(x));}
 public async Task<ServiceResult> CreateAsync(CreateServiceRouteDto r,CancellationToken t=default){await repository.CreateAsync(ToEntity(r),t);return ServiceResult.Success();}
 public async Task<ServiceResult> UpdateAsync(UpdateServiceRouteDto r,CancellationToken t=default){var x=ToEntity(r);x.ServiceRouteId=r.ServiceRouteId;var n=await repository.UpdateAsync(x,t);return n==0?ServiceResult.NotFound("Servis güzergâhı bulunamadı."):ServiceResult.NoContent();}
 public async Task<ServiceResult> DeleteAsync(IdRequest r,CancellationToken t=default){var n=await repository.DeleteAsync(r.Id,t);return n==0?ServiceResult.NotFound("Servis güzergâhı bulunamadı."):ServiceResult.NoContent();}
 private static ServiceRoutes ToEntity(CreateServiceRouteDto r)=>new(){RouteName=r.RouteName,DepartureLocation=r.DepartureLocation,ArrivalLocation=r.ArrivalLocation,RouteDescription=r.RouteDescription,DepartureTime=r.DepartureTime,ArrivalTime=r.ArrivalTime,IsActive=r.IsActive,DisplayOrder=r.DisplayOrder};
 private static ServiceRouteDto ToDto(ServiceRoutes x)=>new(){ServiceRouteId=x.ServiceRouteId,RouteName=x.RouteName,DepartureLocation=x.DepartureLocation,ArrivalLocation=x.ArrivalLocation,RouteDescription=x.RouteDescription,DepartureTime=x.DepartureTime,ArrivalTime=x.ArrivalTime,IsActive=x.IsActive,DisplayOrder=x.DisplayOrder,CreatedAt=x.CreatedAt,UpdatedAt=x.UpdatedAt};
}
