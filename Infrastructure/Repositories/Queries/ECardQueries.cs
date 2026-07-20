namespace Infrastructure.Queries;



public static class ECardQueries

{

    private const string SelectColumns = """

        SELECT

            e_card_id,

            title,

            description,

            card_type,

            image_url,

            redirect_url,

            is_active,

            display_order,

            company_scope,

            company_id,

            branch_scope,

            branch_id,

            department_scope,

            department_id,

            created_at,

            updated_at

        FROM e_cards

        """;



    public const string GetAll = SelectColumns + " ORDER BY display_order ASC, title ASC;";



    public const string GetById = SelectColumns + " WHERE e_card_id = @ECardId;";



    public const string Create = """

        INSERT INTO e_cards

        (

            title, description, card_type, image_url, redirect_url, is_active, display_order,

            company_scope, company_id, branch_scope, branch_id, department_scope, department_id,

            created_at

        )

        VALUES

        (

            @Title, @Description, @CardType, @ImageUrl, @RedirectUrl, @IsActive, @DisplayOrder,

            @CompanyScope, @CompanyId, @BranchScope, @BranchId, @DepartmentScope, @DepartmentId,

            GETDATE()

        );

        """;



    public const string Update = """

        UPDATE e_cards

        SET

            title = @Title,

            description = @Description,

            card_type = @CardType,

            image_url = @ImageUrl,

            redirect_url = @RedirectUrl,

            is_active = @IsActive,

            display_order = @DisplayOrder,

            company_scope = @CompanyScope,

            company_id = @CompanyId,

            branch_scope = @BranchScope,

            branch_id = @BranchId,

            department_scope = @DepartmentScope,

            department_id = @DepartmentId,

            updated_at = GETDATE()

        WHERE e_card_id = @ECardId;

        """;



    public const string SoftDelete = """

        UPDATE e_cards

        SET is_active = 0, updated_at = GETDATE()

        WHERE e_card_id = @ECardId;

        """;

}


