// using Application.DTOs;
// using Application.Services;
// using Microsoft.AspNetCore.Mvc;

// namespace Api.Controllers;

// [ApiController]
// [Route("api/[controller]")]
// public class EventsController : ControllerBase
// {
//     private readonly EventService _eventService;

//     public EventsController(EventService eventService)
//     {
//         _eventService = eventService;
//     }


//     // GET: api/events
//     [HttpGet]
//     public async Task<IActionResult> GetAll(
//         CancellationToken cancellationToken)
//     {
//         var events = await _eventService.GetAllAsync(cancellationToken);

//         return Ok(events);
//     }


//     // GET: api/events/{id}
//     [HttpGet("{id:long}")]
//     public async Task<IActionResult> GetById(
//         long id,
//         CancellationToken cancellationToken)
//     {
//         var eventItem = await _eventService.GetByIdAsync(
//             id,
//             cancellationToken);

//         if (eventItem == null)
//         {
//             return NotFound();
//         }

//         return Ok(eventItem);
//     }


//     // POST: api/events
//     [HttpPost]
//     public async Task<IActionResult> Create(
//         [FromBody] EventDto eventDto,
//         CancellationToken cancellationToken)
//     {
//         await _eventService.CreateAsync(
//             eventDto,
//             cancellationToken);

//         return Ok(new
//         {
//             message = "Event created successfully"
//         });
//     }


//     // PUT: api/events/{id}
//     [HttpPut("{id:long}")]
//     public async Task<IActionResult> Update(
//         long id,
//         [FromBody] EventDto eventDto,
//         CancellationToken cancellationToken)
//     {
//         eventDto.EventId = id;

//         await _eventService.UpdateAsync(
//             eventDto,
//             cancellationToken);

//         return Ok(new
//         {
//             message = "Event updated successfully"
//         });
//     }


//     // DELETE: api/events/{id}
//     [HttpDelete("{id:long}")]
//     public async Task<IActionResult> Delete(
//         long id,
//         CancellationToken cancellationToken)
//     {
//         await _eventService.DeleteAsync(
//             id,
//             cancellationToken);

//         return Ok(new
//         {
//             message = "Event deleted successfully"
//         });
//     }
// }