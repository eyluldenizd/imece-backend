using Application.DTOs;
using Core.Common;

namespace Application.Services;

public sealed class WeatherService
{
    public async Task<ServiceResult<WeatherForecastDto>> GetWeatherByCityAsync(string city, CancellationToken cancellationToken = default)
    {
        await Task.Delay(10, cancellationToken);
        var mockResponse = new WeatherForecastDto
        {
            City = city,
            Temperature = 25,
            Condition = "Sunny",
            Humidity = 50
        };
        return ServiceResult<WeatherForecastDto>.Success(mockResponse);
    }
}
