namespace Core.Common.Validation;

public sealed class ValidationResult
{
    public IReadOnlyList<ValidationError> Errors { get; }

    public bool IsValid => Errors.Count == 0;

    public ValidationResult(
        IEnumerable<ValidationError>? errors = null)
    {
        Errors = errors?
            .Distinct()
            .ToList()
            ?? [];
    }

    public static ValidationResult Success()
    {
        return new ValidationResult();
    }

    public IReadOnlyDictionary<string, string[]>
        ToDictionary()
    {
        return Errors
            .GroupBy(error => error.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group
                    .Select(error => error.ErrorMessage)
                    .Distinct()
                    .ToArray());
    }
}