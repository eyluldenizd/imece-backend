namespace Core.Common.Validation;

public sealed record ValidationError(
    string PropertyName,
    string ErrorMessage);