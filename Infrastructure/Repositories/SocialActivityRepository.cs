using Infrastructure.Database.DataAccess;

using Infrastructure.Entities;

using Infrastructure.Repositories.Queries;

using Microsoft.Data.SqlClient;

using System.Data;



namespace Infrastructure.Repositories;



public sealed class SocialActivityRepository

{

    private readonly ISqlDataAccess _dataAccess;



    public SocialActivityRepository(ISqlDataAccess dataAccess)

    {

        _dataAccess = dataAccess;

    }



    public Task<List<SocialActivityListItem>> GetAllAsync(CancellationToken cancellationToken = default)

        => _dataAccess.QueryAsync<SocialActivityListItem>(SocialActivityQueries.GetAll, cancellationToken: cancellationToken);



    public Task<SocialActivityListItem?> GetByIdAsync(long id, CancellationToken cancellationToken = default)

    {

        SqlParameter[] parameters =

        [

            new("@SocialActivityId", SqlDbType.BigInt) { Value = id }

        ];



        return _dataAccess.QueryFirstOrDefaultAsync<SocialActivityListItem>(

            SocialActivityQueries.GetById,

            parameters,

            cancellationToken);

    }



    public Task<long> CreateAsync(SocialActivities entity, CancellationToken cancellationToken = default)

        => _dataAccess.ExecuteScalarAsync<long>(

            SocialActivityQueries.Create,

            BuildParameters(entity, includeId: false),

            cancellationToken);



    public Task<int> UpdateAsync(SocialActivities entity, CancellationToken cancellationToken = default)

        => _dataAccess.ExecuteAsync(

            SocialActivityQueries.Update,

            BuildParameters(entity, includeId: true),

            cancellationToken);



    public Task<int> SoftDeleteAsync(long id, CancellationToken cancellationToken = default)

    {

        SqlParameter[] parameters =

        [

            new("@SocialActivityId", SqlDbType.BigInt) { Value = id }

        ];



        return _dataAccess.ExecuteAsync(SocialActivityQueries.SoftDelete, parameters, cancellationToken);

    }



    private static SqlParameter[] BuildParameters(SocialActivities entity, bool includeId)

    {

        var list = new List<SqlParameter>();



        if (includeId)

        {

            list.Add(new("@SocialActivityId", entity.SocialActivityId));

        }



        list.Add(new("@Title", entity.Title));

        list.Add(new("@Description", (object?)entity.Description ?? DBNull.Value));

        list.Add(new("@ActivityType", entity.ActivityType));

        list.Add(new("@Location", (object?)entity.Location ?? DBNull.Value));

        list.Add(new("@StartAt", entity.StartAt));

        list.Add(new("@EndAt", entity.EndAt));

        list.Add(new("@ImageUrl", (object?)entity.ImageUrl ?? DBNull.Value));

        list.Add(new("@CompanyScope", entity.CompanyScope));

        list.Add(new("@CompanyId", (object?)entity.CompanyId ?? DBNull.Value));

        list.Add(new("@BranchScope", entity.BranchScope));

        list.Add(new("@BranchId", (object?)entity.BranchId ?? DBNull.Value));

        list.Add(new("@DepartmentScope", entity.DepartmentScope));

        list.Add(new("@DepartmentId", (object?)entity.DepartmentId ?? DBNull.Value));

        list.Add(new("@Status", entity.Status));

        list.Add(new("@IsActive", entity.IsActive));

        list.Add(new("@CreatedBy", entity.CreatedBy));



        return list.ToArray();

    }

}


