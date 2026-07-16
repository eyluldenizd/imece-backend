namespace Infrastructure.Repositories.Queries;

public static class CampaignsQueries
{
    private const string SelectColumns = "SELECT campaign_id AS CampaignId, title AS Title, description AS Description, image_url AS ImageUrl, target_url AS TargetUrl, start_date AS StartDate, end_date AS EndDate, is_active AS IsActive FROM campaigns";
    public const string GetAll = $"{SelectColumns} ORDER BY start_date DESC;";
    public const string GetActive = $"{SelectColumns} WHERE is_active = 1 AND start_date <= SYSDATETIME() AND end_date >= SYSDATETIME() ORDER BY start_date DESC;";
    public const string GetById = $"{SelectColumns} WHERE campaign_id = @CampaignId;";
    public const string Create = "INSERT INTO campaigns (title, description, image_url, target_url, start_date, end_date, is_active) OUTPUT INSERTED.campaign_id VALUES (@Title, @Description, @ImageUrl, @TargetUrl, @StartDate, @EndDate, @IsActive);";
    public const string Update = "UPDATE campaigns SET title = @Title, description = @Description, image_url = @ImageUrl, target_url = @TargetUrl, start_date = @StartDate, end_date = @EndDate, is_active = @IsActive WHERE campaign_id = @CampaignId;";
    public const string Delete = "DELETE FROM campaigns WHERE campaign_id = @CampaignId;";
}
