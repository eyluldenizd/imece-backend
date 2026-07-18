using Application.Services;
using Core.Authorization;
using ImeceWebAPI.Controllers.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ImeceWebAPI.Controllers;

[ApiController]
[Route("api/weather/")]
[Authorize(Policy = ImecePolicies.RequireRegisteredUser)]
public sealed class WeatherController : ApiControllerBase
{
    private readonly WeatherService _weatherService;

    public WeatherController(WeatherService weatherService)
    {
        _weatherService = weatherService;
    }

    [HttpGet("get-weather-by-city/{city}")]
    public Task<IActionResult> Get(string city, CancellationToken cancellationToken)
    {
        return ExecuteAsync(city, _weatherService.GetWeatherByCityAsync, cancellationToken);
    }
}
