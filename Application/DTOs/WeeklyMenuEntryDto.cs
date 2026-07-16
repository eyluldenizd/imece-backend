using Core.Common.Validation;

namespace Application.DTOs;

public sealed class WeeklyMenuEntryDto
{
    public long EntryId { get; set; }

    public int WeekId { get; set; }

    public short Year { get; set; }

    public byte WeekNumber { get; set; }

    public DateOnly WeekStartDate { get; set; }

    public DateOnly WeekEndDate { get; set; }

    public int DishId { get; set; }

    public string DishName { get; set; } = string.Empty;

    public string DishCategory { get; set; } = string.Empty;

    public int BranchId { get; set; }

    public string BranchName { get; set; } = string.Empty;

    public DateOnly MenuDate { get; set; }

    public string MealType { get; set; } = string.Empty;

    public short SortOrder { get; set; }

    public int? CreatedBy { get; set; }

    public string? CreatedByFullName { get; set; }

    public DateTime CreatedAt { get; set; }
}

public sealed class CreateWeeklyMenuEntryDto
{
    [Validate(
        ValidationRuleType.GreaterThan,
        0,
        ErrorMessage = "Geçerli bir hafta ID değeri gönderilmelidir.")]
    public int WeekId { get; set; }

    [Validate(
        ValidationRuleType.GreaterThan,
        0,
        ErrorMessage = "Geçerli bir yemek ID değeri gönderilmelidir.")]
    public int DishId { get; set; }

    [Validate(
        ValidationRuleType.GreaterThan,
        0,
        ErrorMessage = "Geçerli bir şube ID değeri gönderilmelidir.")]
    public int BranchId { get; set; }

    public DateOnly MenuDate { get; set; }

    [Validate(
        ValidationRuleType.Required,
        ErrorMessage = "Öğün tipi zorunludur.")]
    [Validate(
        ValidationRuleType.MaxLength,
        20,
        ErrorMessage = "Öğün tipi en fazla 20 karakter olabilir.")]
    public string MealType { get; set; } = string.Empty;

    public short SortOrder { get; set; }

    public int? CreatedBy { get; set; }
}

public sealed class UpdateWeeklyMenuEntryDto
{
    [Validate(
        ValidationRuleType.GreaterThan,
        0,
        ErrorMessage = "Geçerli bir menü kaydı ID değeri gönderilmelidir.")]
    public long EntryId { get; set; }

    [Validate(
        ValidationRuleType.GreaterThan,
        0,
        ErrorMessage = "Geçerli bir hafta ID değeri gönderilmelidir.")]
    public int WeekId { get; set; }

    [Validate(
        ValidationRuleType.GreaterThan,
        0,
        ErrorMessage = "Geçerli bir yemek ID değeri gönderilmelidir.")]
    public int DishId { get; set; }

    [Validate(
        ValidationRuleType.GreaterThan,
        0,
        ErrorMessage = "Geçerli bir şube ID değeri gönderilmelidir.")]
    public int BranchId { get; set; }

    public DateOnly MenuDate { get; set; }

    [Validate(
        ValidationRuleType.Required,
        ErrorMessage = "Öğün tipi zorunludur.")]
    [Validate(
        ValidationRuleType.MaxLength,
        20,
        ErrorMessage = "Öğün tipi en fazla 20 karakter olabilir.")]
    public string MealType { get; set; } = string.Empty;

    public short SortOrder { get; set; }

    public int? CreatedBy { get; set; }
}

public sealed class WeeklyMenuDateRequest
{
    public DateOnly MenuDate { get; set; }
}

public sealed class WeeklyMenuBranchRequest
{
    [Validate(
        ValidationRuleType.GreaterThan,
        0,
        ErrorMessage = "Geçerli bir şube ID değeri gönderilmelidir.")]
    public int BranchId { get; set; }
}