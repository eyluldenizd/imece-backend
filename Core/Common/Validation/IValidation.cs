using System.ComponentModel.DataAnnotations;

namespace Core.Common.Validation;

public interface IValidator<in T>
{
    ValueTask<ValidationResult> ValidateAsync(
        T instance,
        CancellationToken cancellationToken = default);
}