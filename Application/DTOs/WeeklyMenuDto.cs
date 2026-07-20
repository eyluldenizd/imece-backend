using Core.Common.Validation;

namespace Application.DTOs;

public sealed class WeeklyMenuDto
{
    public long MenuId { get; set; }

    public int CompanyId { get; set; }

    public string MenuCode { get; set; } = string.Empty;

    public int Year { get; set; }

    public int Month { get; set; }

    public int WeekOfMonth { get; set; }

    public DateOnly PeriodStartDate { get; set; }

    public DateOnly PeriodEndDate { get; set; }

    public string? Title { get; set; }

    public bool IsPublished { get; set; }

    public DateTime? PublishedAt { get; set; }

    public bool IsActive { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public IReadOnlyList<WeeklyMenuItemDto> Items { get; set; } = [];
}

public sealed class CreateWeeklyMenuDto
{
    [Validate(ValidationRuleType.GreaterThan, 0, ErrorMessage = "Şirket kimliği sıfırdan büyük olmalıdır.")]
    public int CompanyId { get; set; }

    [Validate(ValidationRuleType.GreaterThan, 0, ErrorMessage = "Yıl sıfırdan büyük olmalıdır.")]
    public int Year { get; set; }

    [Validate(ValidationRuleType.GreaterThanOrEqual, 1, ErrorMessage = "Ay 1 ile 12 arasında olmalıdır.")]
    [Validate(ValidationRuleType.LessThanOrEqual, 12, ErrorMessage = "Ay 1 ile 12 arasında olmalıdır.")]
    public int Month { get; set; }

    [Validate(ValidationRuleType.GreaterThanOrEqual, 1, ErrorMessage = "Hafta 1 ile 5 arasında olmalıdır.")]
    [Validate(ValidationRuleType.LessThanOrEqual, 5, ErrorMessage = "Hafta 1 ile 5 arasında olmalıdır.")]
    public int WeekOfMonth { get; set; }

    [Validate(ValidationRuleType.MaxLength, 256, ErrorMessage = "Başlık en fazla 256 karakter olabilir.")]
    public string? Title { get; set; }
}

public sealed class UpdateWeeklyMenuDto
{
    [Validate(ValidationRuleType.GreaterThan, 0, ErrorMessage = "Menü kimliği sıfırdan büyük olmalıdır.")]
    public long MenuId { get; set; }

    [Validate(ValidationRuleType.MaxLength, 256, ErrorMessage = "Başlık en fazla 256 karakter olabilir.")]
    public string? Title { get; set; }
}

public sealed class WeeklyMenuItemDto
{
    public long MenuItemId { get; set; }

    public long MenuId { get; set; }

    public DateOnly MenuDate { get; set; }

    public int DishCategoryId { get; set; }

    public string? DishCategoryName { get; set; }

    public int DishId { get; set; }

    public string? DishName { get; set; }

    public int SortOrder { get; set; }

    public string? Notes { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

public sealed class CreateWeeklyMenuItemDto
{
    [Validate(ValidationRuleType.GreaterThan, 0, ErrorMessage = "Menü kimliği sıfırdan büyük olmalıdır.")]
    public long MenuId { get; set; }

    public DateOnly MenuDate { get; set; }

    [Validate(ValidationRuleType.GreaterThan, 0, ErrorMessage = "Yemek kategorisi seçilmelidir.")]
    public int DishCategoryId { get; set; }

    [Validate(ValidationRuleType.GreaterThan, 0, ErrorMessage = "Yemek seçilmelidir.")]
    public int DishId { get; set; }

    public int SortOrder { get; set; }

    [Validate(ValidationRuleType.MaxLength, 512, ErrorMessage = "Not en fazla 512 karakter olabilir.")]
    public string? Notes { get; set; }
}

public sealed class UpdateWeeklyMenuItemDto
{
    [Validate(ValidationRuleType.GreaterThan, 0, ErrorMessage = "Menü kimliği sıfırdan büyük olmalıdır.")]
    public long MenuId { get; set; }

    [Validate(ValidationRuleType.GreaterThan, 0, ErrorMessage = "Menü öğesi kimliği sıfırdan büyük olmalıdır.")]
    public long MenuItemId { get; set; }

    public DateOnly MenuDate { get; set; }

    [Validate(ValidationRuleType.GreaterThan, 0, ErrorMessage = "Yemek kategorisi seçilmelidir.")]
    public int DishCategoryId { get; set; }

    [Validate(ValidationRuleType.GreaterThan, 0, ErrorMessage = "Yemek seçilmelidir.")]
    public int DishId { get; set; }

    public int SortOrder { get; set; }

    [Validate(ValidationRuleType.MaxLength, 512, ErrorMessage = "Not en fazla 512 karakter olabilir.")]
    public string? Notes { get; set; }
}

public sealed class WeeklyMenuItemRouteRequest
{
    [Validate(ValidationRuleType.GreaterThan, 0, ErrorMessage = "Menü kimliği sıfırdan büyük olmalıdır.")]
    public long MenuId { get; set; }

    [Validate(ValidationRuleType.GreaterThan, 0, ErrorMessage = "Menü öğesi kimliği sıfırdan büyük olmalıdır.")]
    public long MenuItemId { get; set; }
}

public sealed class WeeklyMenuRouteRequest
{
    [Validate(ValidationRuleType.GreaterThan, 0, ErrorMessage = "Menü kimliği sıfırdan büyük olmalıdır.")]
    public long MenuId { get; set; }
}
