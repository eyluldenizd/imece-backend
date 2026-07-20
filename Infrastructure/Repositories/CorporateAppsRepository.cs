using Core.Entities;
using Infrastructure.Database.DataAccess;
using Infrastructure.Repositories.Queries;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Repositories;

public sealed class CorporateAppsRepository
{
    private readonly ISqlDataAccess _dataAccess;

    public CorporateAppsRepository(ISqlDataAccess dataAccess)
    {
        _dataAccess = dataAccess;
    }

    public Task<List<CorporateApps>> GetAllAsync(CancellationToken cancellationToken = default) 
        => _dataAccess.QueryAsync<CorporateApps>(CorporateAppsQueries.GetAll, null, cancellationToken);

    public Task<CorporateApps?> GetByIdAsync(long id,CancellationToken token=default)=>_dataAccess.QueryFirstOrDefaultAsync<CorporateApps>("SELECT app_id,title,description,url,category,is_active FROM corporate_apps WHERE app_id=@Id",[new("@Id",id)],token);
    public Task<long> CreateAsync(CorporateApps x,CancellationToken token=default)=>_dataAccess.ExecuteScalarAsync<long>("INSERT INTO corporate_apps(title,description,url,category,is_active) OUTPUT INSERTED.app_id VALUES(@Title,@Description,@Url,@Category,1)",Params(x,false),token)!;
    public Task<int> UpdateAsync(CorporateApps x,CancellationToken token=default)=>_dataAccess.ExecuteAsync("UPDATE corporate_apps SET title=@Title,description=@Description,url=@Url,category=@Category WHERE app_id=@Id",Params(x,true),token);
    public Task<int> DeleteAsync(long id,CancellationToken token=default)=>_dataAccess.ExecuteAsync("UPDATE corporate_apps SET is_active=0 WHERE app_id=@Id",[new("@Id",id)],token);
    private static SqlParameter[] Params(CorporateApps x,bool id){var p=new List<SqlParameter>{new("@Title",x.Title),new("@Description",(object?)x.Description??DBNull.Value),new("@Url",x.Url),new("@Category",(object?)x.Category??DBNull.Value)};if(id)p.Add(new("@Id",x.AppId));return p.ToArray();}
}
