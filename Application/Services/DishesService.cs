using Application.DTOs;
using Core.Common;
using Infrastructure.Entities;
using Infrastructure.Repositories;

namespace Application.Services;

public sealed class DishesService(DishesRepository repository)
{
    public async Task<ServiceResult<IReadOnlyList<DishesDto>>> GetAllAsync(CancellationToken t=default){var x=await repository.GetAllAsync(t);return ServiceResult<IReadOnlyList<DishesDto>>.Success(x.Select(ToDto).ToList());}
    public async Task<ServiceResult<IReadOnlyList<DishesDto>>> GetActiveAsync(CancellationToken t=default){var x=await repository.GetAllAsync(t);return ServiceResult<IReadOnlyList<DishesDto>>.Success(x.Where(d=>d.IsActive).Select(ToDto).ToList());}
    public async Task<ServiceResult<DishesDto>> GetByIdAsync(IdRequest r,CancellationToken t=default){var x=await repository.GetByIdAsync(r.Id,t);return x is null?ServiceResult<DishesDto>.NotFound("Yemek bulunamadı."):ServiceResult<DishesDto>.Success(ToDto(x));}
    public async Task<ServiceResult> CreateAsync(CreateDishesDto r,CancellationToken t=default){await repository.CreateAsync(new(){DishName=r.DishName.Trim(),Category=r.Category.Trim(),IsActive=r.IsActive},t);return ServiceResult.Success();}
    public async Task<ServiceResult> UpdateAsync(UpdateDishesDto r,CancellationToken t=default){var n=await repository.UpdateAsync(new(){DishId=r.DishId,DishName=r.DishName.Trim(),Category=r.Category.Trim(),IsActive=r.IsActive},t);return n==0?ServiceResult.NotFound("Yemek bulunamadı."):ServiceResult.NoContent();}
    public async Task<ServiceResult> DeleteAsync(IdRequest r,CancellationToken t=default){var n=await repository.DeleteAsync(r.Id,t);return n==0?ServiceResult.NotFound("Yemek bulunamadı."):ServiceResult.NoContent();}
    private static DishesDto ToDto(Dishes x)=>new(){DishId=x.DishId,DishName=x.DishName,Category=x.Category,IsActive=x.IsActive,CreatedAt=x.CreatedAt};
}
