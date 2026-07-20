namespace Infrastructure.Repositories.Queries;

public static class CommunicationChannelsQueries
{
    private const string SelectColumns = $"""
        SELECT
            t.channel_id AS ChannelId,
            t.channel_name AS ChannelName,
            t.type AS Type,
            t.communication_channel_type_id AS CommunicationChannelTypeId,
            ct.icon_url AS TypeIconUrl,
            t.address_url AS AddressUrl,
            t.department_in_charge AS DepartmentInCharge,
            t.description AS Description,
            t.icon AS Icon,
            t.sort_order AS SortOrder,
            t.is_active AS IsActive,
            {OrganizationScopeSql.SelectColumns},
            {OrganizationScopeSql.ListNameColumns},
            t.created_at AS CreatedAt,
            t.updated_at AS UpdatedAt
        FROM communication_channels AS t
        LEFT JOIN communication_channel_types AS ct
            ON ct.communication_channel_type_id = t.communication_channel_type_id
        {OrganizationScopeSql.ListJoins}
        """;

    public const string GetAll = $"{SelectColumns} ORDER BY t.sort_order ASC, t.channel_name ASC;";

    public const string GetById = $"{SelectColumns} WHERE t.channel_id = @ChannelId;";

    public const string Create = """
        INSERT INTO communication_channels
        (
            channel_name, type, communication_channel_type_id, address_url, department_in_charge,
            description, icon, sort_order, is_active,
            company_scope, company_id, branch_scope, branch_id,
            department_scope, department_id
        )
        OUTPUT INSERTED.channel_id
        VALUES
        (
            @ChannelName, @Type, @CommunicationChannelTypeId, @AddressUrl, @DepartmentInCharge,
            @Description, @Icon, @SortOrder, @IsActive,
            @CompanyScope, @CompanyId, @BranchScope, @BranchId,
            @DepartmentScope, @DepartmentId
        );
        """;

    public const string Update = """
        UPDATE communication_channels
        SET
            channel_name = @ChannelName,
            type = @Type,
            communication_channel_type_id = @CommunicationChannelTypeId,
            address_url = @AddressUrl,
            department_in_charge = @DepartmentInCharge,
            description = @Description,
            icon = @Icon,
            sort_order = @SortOrder,
            is_active = @IsActive,
            company_scope = @CompanyScope,
            company_id = @CompanyId,
            branch_scope = @BranchScope,
            branch_id = @BranchId,
            department_scope = @DepartmentScope,
            department_id = @DepartmentId,
            updated_at = SYSUTCDATETIME()
        WHERE channel_id = @ChannelId;
        """;

    public const string SoftDelete = """
        UPDATE communication_channels
        SET is_active = 0, updated_at = SYSUTCDATETIME()
        WHERE channel_id = @ChannelId;
        """;
}
