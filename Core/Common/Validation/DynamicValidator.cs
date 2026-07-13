using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Core.Common.Validation;

public sealed class DynamicValidator<T>
    : IValidator<T>
{
    private static readonly PropertyValidationMetadata[]
        ValidationMetadata = CreateMetadata();

    public ValueTask<ValidationResult> ValidateAsync(
        T instance,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(instance);

        var errors = new List<ValidationError>();

        foreach (var metadata in ValidationMetadata)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var value =
                metadata.Property.GetValue(instance);

            foreach (var rule in metadata.Rules)
            {
                if (rule.IsValid(value))
                {
                    continue;
                }

                errors.Add(
                    new ValidationError(
                        metadata.Property.Name,
                        rule.ErrorMessage));
            }
        }

        return ValueTask.FromResult(
            new ValidationResult(errors));
    }

    private static PropertyValidationMetadata[]
        CreateMetadata()
    {
        return typeof(T)
            .GetProperties(
                BindingFlags.Instance
                | BindingFlags.Public)
            .Select(property =>
                new PropertyValidationMetadata(
                    property,
                    property
                        .GetCustomAttributes<ValidateAttribute>(
                            inherit: true)
                        .ToArray()))
            .Where(metadata =>
                metadata.Rules.Length > 0)
            .ToArray();
    }

    private sealed record PropertyValidationMetadata(
        PropertyInfo Property,
        ValidateAttribute[] Rules);
}