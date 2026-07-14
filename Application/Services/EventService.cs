// using Application.DTOs;
// using Infrastructure.Repositories;

// namespace Infrastructure.Services;
// namespace Application.Services;

// public sealed class EventService
// {
//     private readonly EventRepository _eventRepository;

//     public EventService(EventRepository eventRepository)
//     {
//         _eventRepository = eventRepository;
//     }


//     public async Task<List<EventDto>> GetAllAsync(
//         CancellationToken cancellationToken = default)
//     {
//         return await _eventRepository.GetAllAsync(
//             cancellationToken);
//     }


//     public async Task<EventDto?> GetByIdAsync(
//         long id,
//         CancellationToken cancellationToken = default)
//     {
//         return await _eventRepository.GetByIdAsync(
//             id,
//             cancellationToken);
//     }


//     public async Task CreateAsync(
//         EventDto eventDto,
//         CancellationToken cancellationToken = default)
//     {
//         await _eventRepository.CreateAsync(
//             eventDto,
//             cancellationToken);
//     }


//     public async Task UpdateAsync(
//         EventDto eventDto,
//         CancellationToken cancellationToken = default)
//     {
//         await _eventRepository.UpdateAsync(
//             eventDto,
//             cancellationToken);
//     }


//     public async Task DeleteAsync(
//         long id,
//         CancellationToken cancellationToken = default)
//     {
//         await _eventRepository.DeleteAsync(
//             id,
//             cancellationToken);
//     }
// }