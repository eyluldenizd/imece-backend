using System.Collections;
using System.Globalization;

namespace Core.Common.Validation;

[AttributeUsage(
    AttributeTargets.Property,
    AllowMultiple = true,
    Inherited = true)]
public sealed class ValidateAttribute : Attribute
{
    public ValidationRuleType RuleType { get; }

    public double? ComparisonValue { get; }

    public string ErrorMessage { get; set; }
        = "Gönderilen değer geçersizdir.";

    public ValidateAttribute(
        ValidationRuleType ruleType)
    {
        RuleType = ruleType;
    }

    public ValidateAttribute(
        ValidationRuleType ruleType,
        double comparisonValue)
    {
        RuleType = ruleType;
        ComparisonValue = comparisonValue;
    }

    public bool IsValid(
        object? value)
    {
        return RuleType switch
        {
            ValidationRuleType.Required =>
                ValidateRequired(value),

            ValidationRuleType.MinLength =>
                ValidateMinLength(value),

            ValidationRuleType.MaxLength =>
                ValidateMaxLength(value),

            ValidationRuleType.GreaterThan =>
                ValidateNumeric(
                    value,
                    (current, comparison) =>
                        current > comparison),

            ValidationRuleType.GreaterThanOrEqual =>
                ValidateNumeric(
                    value,
                    (current, comparison) =>
                        current >= comparison),

            ValidationRuleType.LessThan =>
                ValidateNumeric(
                    value,
                    (current, comparison) =>
                        current < comparison),

            ValidationRuleType.LessThanOrEqual =>
                ValidateNumeric(
                    value,
                    (current, comparison) =>
                        current <= comparison),

            _ => throw new NotSupportedException(
                $"Desteklenmeyen validasyon kuralı: {RuleType}")
        };
    }

    private static bool ValidateRequired(
        object? value)
    {
        if (value is null)
        {
            return false;
        }

        if (value is string stringValue)
        {
            return !string.IsNullOrWhiteSpace(
                stringValue);
        }

        if (value is ICollection collection)
        {
            return collection.Count > 0;
        }

        return true;
    }

    private bool ValidateMinLength(
        object? value)
    {
        if (value is null)
        {
            return true;
        }

        var minimumLength =
            GetComparisonValueAsInt();

        return GetLength(value) >= minimumLength;
    }

    private bool ValidateMaxLength(
        object? value)
    {
        if (value is null)
        {
            return true;
        }

        var maximumLength =
            GetComparisonValueAsInt();

        return GetLength(value) <= maximumLength;
    }

    private bool ValidateNumeric(
        object? value,
        Func<decimal, decimal, bool> comparison)
    {
        if (value is null)
        {
            return true;
        }

        if (!ComparisonValue.HasValue)
                   {
                throw new InvalidOperationException(
                    $"{RuleType} kuralı için karşılaştırma değeri zorunludur.");
            }

        try
        {
            var currentValue =
                Convert.ToDecimal(
                    value,
                    CultureInfo.InvariantCulture);

            var comparisonValue =
                Convert.ToDecimal(
                    ComparisonValue.Value,
                    CultureInfo.InvariantCulture);

            return comparison(
                currentValue,
                comparisonValue);
        }
        catch
        {
            return false;
        }
    }

    private int GetComparisonValueAsInt()
    {
        if (!ComparisonValue.HasValue)
        {
            throw new InvalidOperationException(
                $"{RuleType} kuralı için karşılaştırma değeri zorunludur.");
        }

        return Convert.ToInt32(
            ComparisonValue.Value);
    }

    private static int GetLength(
        object value)
    {
        return value switch
        {
            string stringValue =>
                stringValue.Length,

            ICollection collection =>
                collection.Count,

            _ => throw new InvalidOperationException(
                $"Length validasyonu {value.GetType().Name} tipi için kullanılamaz.")
        };
    }
}