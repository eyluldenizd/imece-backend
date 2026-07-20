namespace Infrastructure.Repositories.Queries;



public static class CampaignsQueries

{

    private const string SelectColumns = $"""

        SELECT

            t.campaign_id AS CampaignId,

            t.title AS Title,

            t.description AS Description,

            t.image_url AS ImageUrl,

            t.target_url AS TargetUrl,

            t.start_date AS StartDate,

            t.end_date AS EndDate,

            t.is_active AS IsActive,

            {OrganizationScopeSql.SelectColumns},

            {OrganizationScopeSql.ListNameColumns}

        FROM campaigns AS t

        {OrganizationScopeSql.ListJoins}

        """;



    public const string GetAll = $"{SelectColumns} ORDER BY t.start_date DESC;";

    public const string GetActive = $"{SelectColumns} WHERE t.is_active = 1 AND t.start_date <= SYSDATETIME() AND t.end_date >= SYSDATETIME() ORDER BY t.start_date DESC;";

    public const string GetById = $"""

        SELECT

            t.campaign_id AS CampaignId,

            t.title AS Title,

            t.description AS Description,

            t.image_url AS ImageUrl,

            t.target_url AS TargetUrl,

            t.start_date AS StartDate,

            t.end_date AS EndDate,

            t.is_active AS IsActive,

            {OrganizationScopeSql.SelectColumns}

        FROM campaigns AS t

        WHERE t.campaign_id = @CampaignId;

        """;

    public const string Create = """

        INSERT INTO campaigns

        (

            title, description, image_url, target_url, start_date, end_date, is_active,

            company_scope, company_id, branch_scope, branch_id, department_scope, department_id

        )

        OUTPUT INSERTED.campaign_id

        VALUES

        (

            @Title, @Description, @ImageUrl, @TargetUrl, @StartDate, @EndDate, @IsActive,

            @CompanyScope, @CompanyId, @BranchScope, @BranchId, @DepartmentScope, @DepartmentId

        );

        """;

    public const string Update = """

        UPDATE campaigns

        SET

            title = @Title,

            description = @Description,

            image_url = @ImageUrl,

            target_url = @TargetUrl,

            start_date = @StartDate,

            end_date = @EndDate,

            is_active = @IsActive,

            company_scope = @CompanyScope,

            company_id = @CompanyId,

            branch_scope = @BranchScope,

            branch_id = @BranchId,

            department_scope = @DepartmentScope,

            department_id = @DepartmentId

        WHERE campaign_id = @CampaignId;

        """;

    /// <summary>Soft-delete: route adı passive; satır silinmez, is_active = 0.</summary>

    public const string SoftDelete = "UPDATE campaigns SET is_active = 0 WHERE campaign_id = @CampaignId;";

}


