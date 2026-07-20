using System.Reflection;
using Application.DTOs;
using Core.Common;
using Infrastructure.Database.DataAccess;
using Microsoft.Data.SqlClient;

namespace Application.Services;

public sealed class AdminRecord
{
    public int Id { get; set; }
    public string? Name { get; set; }
}

public abstract class AdminCrudService
{
    private readonly ISqlDataAccess _sql;
    private readonly string _table;
    private readonly string _idColumn;
    private readonly string _idProperty;

    protected AdminCrudService(ISqlDataAccess sql, string table, string idColumn, string idProperty)
    {
        _sql = sql;
        _table = table;
        _idColumn = idColumn;
        _idProperty = idProperty;
    }

    protected ISqlDataAccess Sql => _sql;

    protected async Task<ServiceResult<IReadOnlyList<AdminRecord>>> GetAllCoreAsync(CancellationToken token)
    {
        var rows = await _sql.QueryAsync<AdminRecord>(
            $"SELECT {_idColumn} AS Id, {GetDisplayColumn()} AS Name FROM {_table} ORDER BY {_idColumn};",
            cancellationToken: token);
        return ServiceResult<IReadOnlyList<AdminRecord>>.Success(rows);
    }

    protected async Task<ServiceResult<AdminRecord>> GetByIdCoreAsync(IdRequest request, CancellationToken token)
    {
        var row = await _sql.QueryFirstOrDefaultAsync<AdminRecord>(
            $"SELECT {_idColumn} AS Id, {GetDisplayColumn()} AS Name FROM {_table} WHERE {_idColumn}=@Id;",
            [new SqlParameter("@Id", request.Id)], token);
        return row is null
            ? ServiceResult<AdminRecord>.NotFound($"ID değeri {request.Id} olan kayıt bulunamadı.")
            : ServiceResult<AdminRecord>.Success(row);
    }

    protected async Task<ServiceResult<long>> CreateCoreAsync<T>(T request, CancellationToken token)
    {
        var values = GetValues(request!, excludeId: true);
        var columns = string.Join(", ", values.Select(x => x.Column));
        var parameterNames = string.Join(", ", values.Select(x => x.Parameter.ParameterName));
        var id = await _sql.ExecuteScalarAsync<long>(
            $"INSERT INTO {_table} ({columns}) OUTPUT INSERTED.{_idColumn} VALUES ({parameterNames});",
            values.Select(x => x.Parameter), token);
        return ServiceResult<long>.Created(id);
    }

    protected async Task<ServiceResult> UpdateCoreAsync<T>(T request, CancellationToken token)
    {
        var id = typeof(T).GetProperty(_idProperty)?.GetValue(request)
            ?? throw new InvalidOperationException($"{_idProperty} alanı bulunamadı.");
        var values = GetValues(request!, excludeId: true);
        var assignments = string.Join(", ", values.Select(x => $"{x.Column}={x.Parameter.ParameterName}"));
        var parameters = values.Select(x => x.Parameter).Append(new SqlParameter("@Id", id)).ToArray();
        var affected = await _sql.ExecuteAsync(
            $"UPDATE {_table} SET {assignments} WHERE {_idColumn}=@Id;", parameters, token);
        return affected == 0 ? ServiceResult.NotFound("Güncellenecek kayıt bulunamadı.") : ServiceResult.NoContent();
    }

    protected async Task<ServiceResult> DeleteCoreAsync(IdRequest request, CancellationToken token)
    {
        var affected = await _sql.ExecuteAsync($"DELETE FROM {_table} WHERE {_idColumn}=@Id;",
            [new SqlParameter("@Id", request.Id)], token);
        return affected == 0 ? ServiceResult.NotFound("Silinecek kayıt bulunamadı.") : ServiceResult.NoContent();
    }

    private string GetDisplayColumn() => _table switch
    {
        "companies" => "company_name",
        "branches" => "branch_name",
        "departments" => "department_name",
        "roles" => "role_name",
        "permissions" => "permission_code",
        "social_activities" => "title",
        "weekly_menus" => "CONVERT(NVARCHAR(64), start_date, 23)",
        _ => "name"
    };

    private List<(string Column, SqlParameter Parameter)> GetValues(object request, bool excludeId)
    {
        return request.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(p => p.CanRead && (!excludeId || !p.Name.Equals(_idProperty, StringComparison.Ordinal)))
            .Select(p => (Column: ToSnakeCase(p.Name), Parameter: new SqlParameter("@" + p.Name, p.GetValue(request) ?? DBNull.Value)))
            .ToList();
    }

    private static string ToSnakeCase(string value) => string.Concat(value.Select((c, i) =>
        char.IsUpper(c) && i > 0 ? "_" + char.ToLowerInvariant(c) : char.ToLowerInvariant(c).ToString()));
}

public sealed class CompanyService(ISqlDataAccess sql) : AdminCrudService(sql, "companies", "company_id", "CompanyId")
{
    public Task<ServiceResult<IReadOnlyList<AdminRecord>>> GetAllAsync(CancellationToken t=default)=>GetAllCoreAsync(t);
    public async Task<ServiceResult<IReadOnlyList<AdminRecord>>> GetActiveAsync(CancellationToken t=default){var r=await Sql.QueryAsync<AdminRecord>("SELECT company_id AS Id, company_name AS Name FROM companies WHERE is_active=1 ORDER BY company_name",cancellationToken:t);return ServiceResult<IReadOnlyList<AdminRecord>>.Success(r);}
    public Task<ServiceResult<AdminRecord>> GetByIdAsync(IdRequest r,CancellationToken t=default)=>GetByIdCoreAsync(r,t);
    public Task<ServiceResult<long>> CreateAsync(CreateCompanyDto r,CancellationToken t=default)=>CreateCoreAsync(r,t);
    public Task<ServiceResult> UpdateAsync(UpdateCompanyDto r,CancellationToken t=default)=>UpdateCoreAsync(r,t);
    public Task<ServiceResult> DeleteAsync(IdRequest r,CancellationToken t=default)=>DeleteCoreAsync(r,t);
}

public sealed class BranchService(ISqlDataAccess sql) : AdminCrudService(sql,"branches","branch_id","BranchId")
{
    public Task<ServiceResult<IReadOnlyList<AdminRecord>>> GetAllAsync(CancellationToken t=default)=>GetAllCoreAsync(t);
    public async Task<ServiceResult<IReadOnlyList<AdminRecord>>> GetActiveAsync(CancellationToken t=default){var x=await Sql.QueryAsync<AdminRecord>("SELECT branch_id AS Id, branch_name AS Name FROM branches WHERE is_active=1",cancellationToken:t);return ServiceResult<IReadOnlyList<AdminRecord>>.Success(x);}
    public async Task<ServiceResult<IReadOnlyList<AdminRecord>>> GetByCompanyIdAsync(CompanyIdRequest r,CancellationToken t=default){var x=await Sql.QueryAsync<AdminRecord>("SELECT branch_id AS Id, branch_name AS Name FROM branches WHERE company_id=@CompanyId",[new("@CompanyId",r.CompanyId)],t);return ServiceResult<IReadOnlyList<AdminRecord>>.Success(x);}
    public Task<ServiceResult<AdminRecord>> GetByIdAsync(IdRequest r,CancellationToken t=default)=>GetByIdCoreAsync(r,t);
    public Task<ServiceResult<long>> CreateAsync(CreateBranchDto r,CancellationToken t=default)=>CreateCoreAsync(r,t);
    public Task<ServiceResult> UpdateAsync(UpdateBranchDto r,CancellationToken t=default)=>UpdateCoreAsync(r,t);
    public Task<ServiceResult> DeleteAsync(IdRequest r,CancellationToken t=default)=>DeleteCoreAsync(r,t);
}

public sealed class DepartmentService(ISqlDataAccess sql) : AdminCrudService(sql,"departments","department_id","DepartmentId")
{
    public Task<ServiceResult<IReadOnlyList<AdminRecord>>> GetAllAsync(CancellationToken t=default)=>GetAllCoreAsync(t);
    private async Task<ServiceResult<IReadOnlyList<AdminRecord>>> Filter(string column,int id,CancellationToken t){var x=await Sql.QueryAsync<AdminRecord>($"SELECT department_id AS Id, department_name AS Name FROM departments WHERE {column}=@Id",[new("@Id",id)],t);return ServiceResult<IReadOnlyList<AdminRecord>>.Success(x);}
    public Task<ServiceResult<IReadOnlyList<AdminRecord>>> GetActiveAsync(CancellationToken t=default)=>Filter("is_active",1,t);
    public Task<ServiceResult<IReadOnlyList<AdminRecord>>> GetByBranchIdAsync(BranchIdRequest r,CancellationToken t=default)=>Filter("branch_id",r.BranchId,t);
    public Task<ServiceResult<IReadOnlyList<AdminRecord>>> GetByCompanyIdAsync(CompanyIdRequest r,CancellationToken t=default)=>Filter("company_id",r.CompanyId,t);
    public Task<ServiceResult<AdminRecord>> GetByIdAsync(IdRequest r,CancellationToken t=default)=>GetByIdCoreAsync(r,t);
    public Task<ServiceResult<long>> CreateAsync(CreateDepartmentDto r,CancellationToken t=default)=>CreateCoreAsync(r,t);
    public Task<ServiceResult> UpdateAsync(UpdateDepartmentDto r,CancellationToken t=default)=>UpdateCoreAsync(r,t);
    public Task<ServiceResult> DeleteAsync(IdRequest r,CancellationToken t=default)=>DeleteCoreAsync(r,t);
}

public abstract class NamedLookupService<TCreate,TUpdate>(ISqlDataAccess sql,string table,string idColumn,string idProperty):AdminCrudService(sql,table,idColumn,idProperty)
{
    public Task<ServiceResult<IReadOnlyList<AdminRecord>>> GetAllAsync(CancellationToken t=default)=>GetAllCoreAsync(t);
    public Task<ServiceResult<AdminRecord>> GetByIdAsync(IdRequest r,CancellationToken t=default)=>GetByIdCoreAsync(r,t);
    public Task<ServiceResult<long>> CreateInternal(TCreate r,CancellationToken t)=>CreateCoreAsync(r,t);
    public Task<ServiceResult> UpdateInternal(TUpdate r,CancellationToken t)=>UpdateCoreAsync(r,t);
    public Task<ServiceResult> DeleteAsync(IdRequest r,CancellationToken t=default)=>DeleteCoreAsync(r,t);
}
public sealed class CommunicationChannelTypeService(ISqlDataAccess s):NamedLookupService<CreateCommunicationChannelTypeDto,UpdateCommunicationChannelTypeDto>(s,"communication_channel_types","communication_channel_type_id","CommunicationChannelTypeId"){public Task<ServiceResult<long>> CreateAsync(CreateCommunicationChannelTypeDto r,CancellationToken t=default)=>CreateInternal(r,t);public Task<ServiceResult> UpdateAsync(UpdateCommunicationChannelTypeDto r,CancellationToken t=default)=>UpdateInternal(r,t);}
public sealed class CorporateAppCategoryService(ISqlDataAccess s):NamedLookupService<CreateCorporateAppCategoryDto,UpdateCorporateAppCategoryDto>(s,"corporate_app_categories","corporate_app_category_id","CorporateAppCategoryId"){public Task<ServiceResult<long>> CreateAsync(CreateCorporateAppCategoryDto r,CancellationToken t=default)=>CreateInternal(r,t);public Task<ServiceResult> UpdateAsync(UpdateCorporateAppCategoryDto r,CancellationToken t=default)=>UpdateInternal(r,t);}
public sealed class DishCategoryService(ISqlDataAccess s):NamedLookupService<CreateDishCategoryDto,UpdateDishCategoryDto>(s,"dish_categories","dish_category_id","DishCategoryId"){public Task<ServiceResult<long>> CreateAsync(CreateDishCategoryDto r,CancellationToken t=default)=>CreateInternal(r,t);public Task<ServiceResult> UpdateAsync(UpdateDishCategoryDto r,CancellationToken t=default)=>UpdateInternal(r,t);}
public sealed class ServiceLocationTypeService(ISqlDataAccess s):NamedLookupService<CreateServiceLocationTypeDto,UpdateServiceLocationTypeDto>(s,"service_location_types","service_location_type_id","ServiceLocationTypeId"){public Task<ServiceResult<long>> CreateAsync(CreateServiceLocationTypeDto r,CancellationToken t=default)=>CreateInternal(r,t);public Task<ServiceResult> UpdateAsync(UpdateServiceLocationTypeDto r,CancellationToken t=default)=>UpdateInternal(r,t);}

public sealed class MeetingRoomService(ISqlDataAccess s):NamedLookupService<CreateMeetingRoomDto,UpdateMeetingRoomDto>(s,"meeting_rooms","meeting_room_id","MeetingRoomId"){public Task<ServiceResult<long>> CreateAsync(CreateMeetingRoomDto r,CancellationToken t=default)=>CreateInternal(r,t);public Task<ServiceResult> UpdateAsync(UpdateMeetingRoomDto r,CancellationToken t=default)=>UpdateInternal(r,t);}
public sealed class SocialActivityService(ISqlDataAccess s):NamedLookupService<CreateSocialActivityDto,UpdateSocialActivityDto>(s,"social_activities","social_activity_id","SocialActivityId"){public Task<ServiceResult<long>> CreateAsync(CreateSocialActivityDto r,CancellationToken t=default)=>CreateInternal(r,t);public Task<ServiceResult> UpdateAsync(UpdateSocialActivityDto r,CancellationToken t=default)=>UpdateInternal(r,t);}
public sealed class ServiceLocationService(ISqlDataAccess s):NamedLookupService<CreateServiceLocationDto,UpdateServiceLocationDto>(s,"service_locations","service_location_id","ServiceLocationId"){public Task<ServiceResult<long>> CreateAsync(CreateServiceLocationDto r,CancellationToken t=default)=>CreateInternal(r,t);public Task<ServiceResult> UpdateAsync(UpdateServiceLocationDto r,CancellationToken t=default)=>UpdateInternal(r,t);}

public sealed class PermissionService(ISqlDataAccess sql)
{public async Task<ServiceResult<IReadOnlyList<AdminRecord>>> GetAllAsync(CancellationToken t=default){var x=await sql.QueryAsync<AdminRecord>("SELECT permission_id AS Id, permission_code AS Name FROM permissions ORDER BY permission_code",cancellationToken:t);return ServiceResult<IReadOnlyList<AdminRecord>>.Success(x);}}
public sealed class DashboardService(ISqlDataAccess sql)
{public async Task<ServiceResult<object>> GetSummaryAsync(CancellationToken t=default){var users=await sql.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM users",cancellationToken:t);var companies=await sql.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM companies WHERE is_active=1",cancellationToken:t);return ServiceResult<object>.Success(new {UserCount=users,ActiveCompanyCount=companies});}}

public sealed class RoleService(ISqlDataAccess sql):AdminCrudService(sql,"roles","role_id","RoleId")
{
 public Task<ServiceResult<IReadOnlyList<AdminRecord>>> GetAllAsync(CancellationToken t=default)=>GetAllCoreAsync(t); public Task<ServiceResult<AdminRecord>> GetByIdAsync(IdRequest r,CancellationToken t=default)=>GetByIdCoreAsync(r,t); public Task<ServiceResult<long>> CreateAsync(CreateRoleDto r,CancellationToken t=default)=>CreateCoreAsync(r,t); public Task<ServiceResult> UpdateAsync(UpdateRoleDto r,CancellationToken t=default)=>UpdateCoreAsync(r,t); public Task<ServiceResult> DeleteAsync(IdRequest r,CancellationToken t=default)=>DeleteCoreAsync(r,t);
 public async Task<ServiceResult> UpdatePermissionsAsync(UpdateRolePermissionsRequest r,CancellationToken t=default){var ps=r.PermissionIds.Select((id,i)=>new SqlParameter("@P"+i,id)).ToArray();var inserts=string.Join(";",ps.Select((p,i)=>$"INSERT INTO role_permissions(role_id,permission_id) VALUES(@RoleId,{p.ParameterName})"));await Sql.ExecuteAsync($"DELETE FROM role_permissions WHERE role_id=@RoleId;{inserts};",ps.Append(new("@RoleId",r.RoleId)),t);return ServiceResult.NoContent();}
}

public sealed class WeeklyMenuService(ISqlDataAccess s):NamedLookupService<CreateWeeklyMenuDto,UpdateWeeklyMenuDto>(s,"weekly_menus","menu_id","MenuId")
{public Task<ServiceResult<long>> CreateAsync(CreateWeeklyMenuDto r,CancellationToken t=default)=>CreateInternal(r,t);public Task<ServiceResult> UpdateAsync(UpdateWeeklyMenuDto r,CancellationToken t=default)=>UpdateInternal(r,t);public Task<ServiceResult> PublishAsync(WeeklyMenuRouteRequest r,CancellationToken t=default)=>SetPublished(r,true,t);public Task<ServiceResult> UnpublishAsync(WeeklyMenuRouteRequest r,CancellationToken t=default)=>SetPublished(r,false,t);private async Task<ServiceResult> SetPublished(WeeklyMenuRouteRequest r,bool v,CancellationToken t){await Sql.ExecuteAsync("UPDATE weekly_menus SET is_published=@Value WHERE menu_id=@Id",[new("@Value",v),new("@Id",r.MenuId)],t);return ServiceResult.NoContent();}}
public sealed class WeeklyMenuItemService(ISqlDataAccess s):AdminCrudService(s,"weekly_menu_items","menu_item_id","MenuItemId")
{public Task<ServiceResult<long>> CreateAsync(CreateWeeklyMenuItemDto r,CancellationToken t=default)=>CreateCoreAsync(r,t);public Task<ServiceResult> UpdateAsync(UpdateWeeklyMenuItemDto r,CancellationToken t=default)=>UpdateCoreAsync(r,t);public async Task<ServiceResult> DeleteAsync(WeeklyMenuItemRouteRequest r,CancellationToken t=default){var n=await Sql.ExecuteAsync("DELETE FROM weekly_menu_items WHERE menu_id=@MenuId AND menu_item_id=@ItemId",[new("@MenuId",r.MenuId),new("@ItemId",r.MenuItemId)],t);return n==0?ServiceResult.NotFound("Silinecek menü öğesi bulunamadı."):ServiceResult.NoContent();}}
