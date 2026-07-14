// using Infrastructure.Data;
// using Microsoft.Data.SqlClient;
// using System.Data;
// using Infrastructure.Queries;
// using Application.DTOs;

// namespace Infrastructure.Repositories;

// public sealed class EventRepository
// {
//     private readonly DbManager _dbManager;

//     public EventRepository(DbManager dbManager)
//     {
//         _dbManager = dbManager;
//     }


//     public Task<List<EventDto>> GetAllAsync(
//         CancellationToken cancellationToken = default)
//     {
//         return _dbManager.QueryAsync(
//             EventQueries.GetAll,
//             reader => new EventDto
//             {
//                 EventId = reader.GetInt64(
//                     reader.GetOrdinal("event_id")),

//                 Title = reader.GetString(
//                     reader.GetOrdinal("title")),

//                 Description = reader.IsDBNull(
//                     reader.GetOrdinal("description"))
//                     ? null
//                     : reader.GetString(
//                         reader.GetOrdinal("description")),

//                 EventType = reader.IsDBNull(
//                     reader.GetOrdinal("event_type"))
//                     ? null
//                     : reader.GetString(
//                         reader.GetOrdinal("event_type")),

//                 Location = reader.IsDBNull(
//                     reader.GetOrdinal("location"))
//                     ? null
//                     : reader.GetString(
//                         reader.GetOrdinal("location")),

//                 CoverImageUrl = reader.IsDBNull(
//                     reader.GetOrdinal("cover_image_url"))
//                     ? null
//                     : reader.GetString(
//                         reader.GetOrdinal("cover_image_url")),

//                 StartDatetime = reader.GetDateTime(
//                     reader.GetOrdinal("start_datetime")),

//                 EndDatetime = reader.GetDateTime(
//                     reader.GetOrdinal("end_datetime")),

//                 IsAllDay = reader.GetBoolean(
//                     reader.GetOrdinal("is_all_day")),

//                 CreatedBy = reader.GetInt32(
//                     reader.GetOrdinal("created_by")),

//                 CreatedAt = reader.GetDateTime(
//                     reader.GetOrdinal("created_at"))
//             },
//             cancellationToken: cancellationToken);
//     }


//     public Task<EventDto?> GetByIdAsync(
//         long id,
//         CancellationToken cancellationToken = default)
//     {
//         var parameters = new[]
//         {
//             new SqlParameter(
//                 "@EventId",
//                 SqlDbType.BigInt)
//             {
//                 Value = id
//             }
//         };

//         return _dbManager.QueryFirstOrDefaultAsync(
//             EventQueries.GetById,
//             reader => new EventDto
//             {
//                 EventId = reader.GetInt64(reader.GetOrdinal("event_id")),
//                 Title = reader.GetString(reader.GetOrdinal("title")),
//                 Description = reader.IsDBNull(reader.GetOrdinal("description"))
//                     ? null
//                     : reader.GetString(reader.GetOrdinal("description")),
//                 EventType = reader.IsDBNull(reader.GetOrdinal("event_type"))
//                     ? null
//                     : reader.GetString(reader.GetOrdinal("event_type")),
//                 Location = reader.IsDBNull(reader.GetOrdinal("location"))
//                     ? null
//                     : reader.GetString(reader.GetOrdinal("location")),
//                 CoverImageUrl = reader.IsDBNull(reader.GetOrdinal("cover_image_url"))
//                     ? null
//                     : reader.GetString(reader.GetOrdinal("cover_image_url")),
//                 StartDatetime = reader.GetDateTime(reader.GetOrdinal("start_datetime")),
//                 EndDatetime = reader.GetDateTime(reader.GetOrdinal("end_datetime")),
//                 IsAllDay = reader.GetBoolean(reader.GetOrdinal("is_all_day")),
//                 CreatedBy = reader.GetInt32(reader.GetOrdinal("created_by")),
//                 CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at"))
//             },
//             parameters,
//             cancellationToken);
//     }


//     public Task<int> CreateAsync(
//         EventDto eventDto,
//         CancellationToken cancellationToken = default)
//     {
//         var parameters = new[]
//         {
//             new SqlParameter("@Title", eventDto.Title),

//             new SqlParameter("@Description",
//                 (object?)eventDto.Description ?? DBNull.Value),

//             new SqlParameter("@EventType",
//                 (object?)eventDto.EventType ?? DBNull.Value),

//             new SqlParameter("@Location",
//                 (object?)eventDto.Location ?? DBNull.Value),

//             new SqlParameter("@CoverImageUrl",
//                 (object?)eventDto.CoverImageUrl ?? DBNull.Value),

//             new SqlParameter("@StartDatetime",
//                 eventDto.StartDatetime),

//             new SqlParameter("@EndDatetime",
//                 eventDto.EndDatetime),

//             new SqlParameter("@IsAllDay",
//                 eventDto.IsAllDay),

//             new SqlParameter("@CreatedBy",
//                 eventDto.CreatedBy)
//         };


//         return _dbManager.ExecuteAsync(
//             EventQueries.Create,
//             parameters,
//             cancellationToken);
//     }


//     public Task<int> UpdateAsync(
//         EventDto eventDto,
//         CancellationToken cancellationToken = default)
//     {
//         var parameters = new[]
//         {
//             new SqlParameter("@EventId",
//                 SqlDbType.BigInt)
//             {
//                 Value = eventDto.EventId
//             },

//             new SqlParameter("@Title",
//                 eventDto.Title),

//             new SqlParameter("@Description",
//                 (object?)eventDto.Description ?? DBNull.Value),

//             new SqlParameter("@EventType",
//                 (object?)eventDto.EventType ?? DBNull.Value),

//             new SqlParameter("@Location",
//                 (object?)eventDto.Location ?? DBNull.Value),

//             new SqlParameter("@CoverImageUrl",
//                 (object?)eventDto.CoverImageUrl ?? DBNull.Value),

//             new SqlParameter("@StartDatetime",
//                 eventDto.StartDatetime),

//             new SqlParameter("@EndDatetime",
//                 eventDto.EndDatetime),

//             new SqlParameter("@IsAllDay",
//                 eventDto.IsAllDay)
//         };


//         return _dbManager.ExecuteAsync(
//             EventQueries.Update,
//             parameters,
//             cancellationToken);
//     }


//     public Task<int> DeleteAsync(
//         long id,
//         CancellationToken cancellationToken = default)
//     {
//         var parameters = new[]
//         {
//             new SqlParameter(
//                 "@EventId",
//                 SqlDbType.BigInt)
//             {
//                 Value = id
//             }
//         };


//         return _dbManager.ExecuteAsync(
//             EventQueries.Delete,
//             parameters,
//             cancellationToken);
//     }
// }