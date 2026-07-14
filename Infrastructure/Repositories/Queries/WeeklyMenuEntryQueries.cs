namespace Infrastructure.Repositories.Queries;

public static class WeeklyMenuEntryQueries
{
    private const string SelectDetails = """
        SELECT
            wme.entry_id,
            wme.week_id,
            w.year AS week_year,
            w.week_number,
            w.start_date AS week_start_date,
            w.end_date AS week_end_date,
            wme.dish_id,
            d.dish_name,
            d.category AS dish_category,
            wme.branch_id,
            b.branch_name,
            wme.menu_date,
            wme.meal_type,
            wme.sort_order,
            wme.created_by,
            u.full_name AS created_by_full_name,
            wme.created_at
        FROM weekly_menu_entries AS wme
        INNER JOIN weeks AS w
            ON w.week_id = wme.week_id
        INNER JOIN dishes AS d
            ON d.dish_id = wme.dish_id
        INNER JOIN branches AS b
            ON b.branch_id = wme.branch_id
        LEFT JOIN users AS u
            ON u.user_id = wme.created_by

        """;

    public static readonly string GetAll =
        SelectDetails +
        """
        ORDER BY
            wme.menu_date ASC,
            b.branch_name ASC,
            wme.meal_type ASC,
            wme.sort_order ASC;
        """;

    public static readonly string GetById =
        SelectDetails +
        """
        WHERE wme.entry_id = @EntryId;
        """;

    public static readonly string GetCurrentWeek =
        SelectDetails +
        """
        WHERE
            CAST(SYSDATETIME() AS date)
                BETWEEN w.start_date AND w.end_date
        ORDER BY
            wme.menu_date ASC,
            b.branch_name ASC,
            wme.meal_type ASC,
            wme.sort_order ASC;
        """;

    public static readonly string GetByDate =
        SelectDetails +
        """
        WHERE wme.menu_date = @MenuDate
        ORDER BY
            b.branch_name ASC,
            wme.meal_type ASC,
            wme.sort_order ASC;
        """;

    public static readonly string GetByBranch =
        SelectDetails +
        """
        WHERE wme.branch_id = @BranchId
        ORDER BY
            wme.menu_date ASC,
            wme.meal_type ASC,
            wme.sort_order ASC;
        """;

    public const string IsMenuDateInWeek = """
        SELECT COUNT(1)
        FROM weeks
        WHERE
            week_id = @WeekId
            AND @MenuDate BETWEEN start_date AND end_date;
        """;

    public const string Create = """
        INSERT INTO weekly_menu_entries
        (
            week_id,
            dish_id,
            branch_id,
            menu_date,
            meal_type,
            sort_order,
            created_by
        )
        OUTPUT INSERTED.entry_id
        VALUES
        (
            @WeekId,
            @DishId,
            @BranchId,
            @MenuDate,
            @MealType,
            @SortOrder,
            @CreatedBy
        );
        """;

    public const string Update = """
        UPDATE weekly_menu_entries
        SET
            week_id = @WeekId,
            dish_id = @DishId,
            branch_id = @BranchId,
            menu_date = @MenuDate,
            meal_type = @MealType,
            sort_order = @SortOrder,
            created_by = @CreatedBy
        WHERE entry_id = @EntryId;
        """;

    public const string Delete = """
        DELETE FROM weekly_menu_entries
        WHERE entry_id = @EntryId;
        """;
}