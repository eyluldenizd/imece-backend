namespace Infrastructure.Repositories.Queries;

public static class CorporateAppsQueries
{
    public const string GetAll = "SELECT app_id, title, description, url, category, is_active FROM corporate_apps WHERE is_active = 1;";
}
