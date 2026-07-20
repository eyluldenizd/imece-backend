// using Application.DTOs;
// using Infrastructure.Database.DataAccess;
// using Microsoft.Data.SqlClient;
// using System.Data;

// namespace Infrastructure.Repositories;

// public sealed class EventParticipantRepository
// {
//     private readonly ISqlDataAccess _dataAccess;

//     public EventParticipantRepository(ISqlDataAccess dataAccess)
//     {
//         _dataAccess = dataAccess;
//     }


//     public Task<List<EventParticipantDto>> GetByEventIdAsync(
//         long eventId,
//         CancellationToken cancellationToken = default)
//     {
//         const string sql = """
//             SELECT
//                 event_id,
//                 user_id,
//                 status,
//                 registered_at
//             FROM event_participants
//             WHERE event_id = @EventId;
//             """;


//         var parameters = new[]
//         {
//             new SqlParameter(
//                 "@EventId",
//                 SqlDbType.BigInt)
//             {
//                 Value = eventId
//             }
//         };


//         return _dataAccess.QueryAsync(
//             sql,
//             reader => new EventParticipantDto
//             {
//                 EventId = reader.GetInt64(
//                     reader.GetOrdinal("event_id")),

//                 UserId = reader.GetInt32(
//                     reader.GetOrdinal("user_id")),

//                 Status = reader.GetString(
//                     reader.GetOrdinal("status")),

//                 RegisteredAt = reader.GetDateTime(
//                     reader.GetOrdinal("registered_at"))
//             },
//             parameters,
//             cancellationToken);
//     }


//     public Task<int> CreateAsync(
//         EventParticipantDto participant,
//         CancellationToken cancellationToken = default)
//     {
//         const string sql = """
//             INSERT INTO event_participants
//             (
//                 event_id,
//                 user_id,
//                 status,
//                 registered_at
//             )
//             VALUES
//             (
//                 @EventId,
//                 @UserId,
//                 @Status,
//                 @RegisteredAt
//             );
//             """;


//         var parameters = new[]
//         {
//             new SqlParameter(
//                 "@EventId",
//                 SqlDbType.BigInt)
//             {
//                 Value = participant.EventId
//             },

//             new SqlParameter(
//                 "@UserId",
//                 SqlDbType.Int)
//             {
//                 Value = participant.UserId
//             },

//             new SqlParameter(
//                 "@Status",
//                 SqlDbType.NVarChar)
//             {
//                 Value = participant.Status
//             },

//             new SqlParameter(
//                 "@RegisteredAt",
//                 SqlDbType.DateTime)
//             {
//                 Value = participant.RegisteredAt
//             }
//         };


//         return _dataAccess.ExecuteAsync(
//             sql,
//             parameters,
//             cancellationToken);
//     }


//     public Task<int> UpdateStatusAsync(
//         long eventId,
//         int userId,
//         string status,
//         CancellationToken cancellationToken = default)
//     {
//         const string sql = """
//             UPDATE event_participants
//             SET
//                 status = @Status
//             WHERE
//                 event_id = @EventId
//                 AND user_id = @UserId;
//             """;


//         var parameters = new[]
//         {
//             new SqlParameter(
//                 "@EventId",
//                 SqlDbType.BigInt)
//             {
//                 Value = eventId
//             },

//             new SqlParameter(
//                 "@UserId",
//                 SqlDbType.Int)
//             {
//                 Value = userId
//             },

//             new SqlParameter(
//                 "@Status",
//                 SqlDbType.NVarChar)
//             {
//                 Value = status
//             }
//         };


//         return _dataAccess.ExecuteAsync(
//             sql,
//             parameters,
//             cancellationToken);
//     }


//     public Task<int> DeleteAsync(
//         long eventId,
//         int userId,
//         CancellationToken cancellationToken = default)
//     {
//         const string sql = """
//             DELETE FROM event_participants
//             WHERE
//                 event_id = @EventId
//                 AND user_id = @UserId;
//             """;


//         var parameters = new[]
//         {
//             new SqlParameter(
//                 "@EventId",
//                 SqlDbType.BigInt)
//             {
//                 Value = eventId
//             },

//             new SqlParameter(
//                 "@UserId",
//                 SqlDbType.Int)
//             {
//                 Value = userId
//             }
//         };


//         return _dataAccess.ExecuteAsync(
//             sql,
//             parameters,
//             cancellationToken);
//     }
// }