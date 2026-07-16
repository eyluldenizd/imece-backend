namespace Infrastructure.Repositories.Queries;

public static class ServicesQueries
{
    private const string SelectColumns = "SELECT service_id AS ServiceId, name AS Name, description AS Description, icon AS Icon, is_active AS IsActive, created_at AS CreatedAt, updated_at AS UpdatedAt FROM services";
    public const string GetAll = $"{SelectColumns} ORDER BY created_at DESC;";
    public const string GetActive = $"{SelectColumns} WHERE is_active = 1 ORDER BY created_at DESC;";
    public const string GetById = $"{SelectColumns} WHERE service_id = @ServiceId;";
    public const string Create = "INSERT INTO services (name, description, icon, is_active) OUTPUT INSERTED.service_id VALUES (@Name, @Description, @Icon, @IsActive);";
    public const string Update = "UPDATE services SET name = @Name, description = @Description, icon = @Icon, is_active = @IsActive, updated_at = SYSDATETIME() WHERE service_id = @ServiceId;";
    public const string Delete = "DELETE FROM services WHERE service_id = @ServiceId;";
}
