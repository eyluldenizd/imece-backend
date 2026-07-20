namespace Application.Common.CompanyScope;

public static class ContentScopeQueryFilters
{
    public const string ListFilter = """
        (@CompanyId IS NULL OR company_id = @CompanyId OR scope_type = N'Global')
        """;
}
