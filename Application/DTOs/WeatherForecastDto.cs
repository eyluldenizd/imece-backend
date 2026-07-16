using Core.Common.Validation;

namespace Application.DTOs;

public sealed class WeatherForecastDto
{
    [Validate(ValidationRuleType.Required, ErrorMessage = "Şehir adı zorunludur.")]
    public string City { get; set; } = string.Empty;
    public int Temperature { get; set; }
    public string? Condition { get; set; }
    public int Humidity { get; set; }
}
