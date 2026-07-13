using Application.DTOs;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Imece_Backend.Controllers;

[ApiController]
[Route("api/dishes")]
[Authorize]
public sealed class DishesController : ControllerBase
{
    private readonly DishesService _dishesService;

    public DishesController(DishesService dishesService)
    {
        _dishesService = dishesService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<DishesDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<DishesDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        var dishes = await _dishesService.GetAllAsync(cancellationToken);

        return Ok(dishes);
    }

    [HttpGet("active")]
    [ProducesResponseType(typeof(List<DishesDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<DishesDto>>> GetActive(
        CancellationToken cancellationToken)
    {
        var dishes = await _dishesService.GetActiveAsync(cancellationToken);

        return Ok(dishes);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(DishesDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DishesDto>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var dish = await _dishesService.GetByIdAsync(
            id,
            cancellationToken);

        return Ok(dish);
    }

    //[HttpPost]
    //[ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    //[ProducesResponseType(StatusCodes.Status400BadRequest)]
    //public async Task<IActionResult> Create(
    //    [FromBody] DishesDto dto,
    //    CancellationToken cancellationToken)
    //{
    //    var dishId = await _dishesService.CreateAsync(
    //        dto,
    //        cancellationToken);

    //    return CreatedAtAction(
    //        nameof(GetById),
    //        new { id = dishId },
    //        new
    //        {
    //            DishId = dishId,
    //            Message = "Yemek başarıyla oluşturuldu."
    //        });
    //}

    //[HttpPut("{id:int}")]
    //[ProducesResponseType(StatusCodes.Status204NoContent)]
    //[ProducesResponseType(StatusCodes.Status400BadRequest)]
    //[ProducesResponseType(StatusCodes.Status404NotFound)]
    //public async Task<IActionResult> Update(
    //    int id,
    //    [FromBody] DishesDto dto,
    //    CancellationToken cancellationToken)
    //{
    //    if (id != dto.DishId)
    //    {
    //        return BadRequest(new
    //        {
    //            Message = "URL üzerindeki yemek kimliği ile gönderilen yemek kimliği eşleşmiyor."
    //        });
    //    }

    //    await _dishesService.UpdateAsync(
    //        dto,
    //        cancellationToken);

    //    return NoContent();
    //}

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken cancellationToken)
    {
        await _dishesService.DeleteAsync(
            id,
            cancellationToken);

        return NoContent();
    }
}