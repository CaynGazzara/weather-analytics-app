using Microsoft.AspNetCore.Mvc;
using WeatherAnalytics.Core.Interfaces;
using WeatherAnalytics.Core.Models;

namespace WeatherAnalytics.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WeatherController : ControllerBase
    {
        private readonly IWeatherService _weatherService;

        public WeatherController(IWeatherService weatherService)
        {
            _weatherService = weatherService;
        }

        [HttpGet("current/{city}")]
        public async Task<ActionResult<WeatherData>> GetCurrentWeather(string city, [FromQuery] string country = "")
        {
            try
            {
                var weather = await _weatherService.GetCurrentWeather(city, country);
                return Ok(weather);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao buscar dados climáticos: {ex.Message}");
            }
        }

        [HttpGet("forecast/{city}")]
        public async Task<ActionResult<WeatherForecast>> GetForecast(string city, [FromQuery] string country = "", [FromQuery] int days = 5)
        {
            try
            {
                var forecast = await _weatherService.GetWeatherForecast(city, country, days);
                return Ok(forecast);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao buscar previsão: {ex.Message}");
            }
        }

        [HttpGet("historical/{city}")]
        public async Task<ActionResult<List<HistoricalWeatherData>>> GetHistoricalWeather(
            string city,
            [FromQuery] DateTime fromDate,
            [FromQuery] DateTime toDate)
        {
            try
            {
                var historicalData = await _weatherService.GetHistoricalWeather(city, fromDate, toDate);
                return Ok(historicalData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao buscar dados históricos: {ex.Message}");
            }
        }

        [HttpPost("compare")]
        public async Task<ActionResult<CityComparison>> CompareCities([FromBody] List<string> cities)
        {
            try
            {
                var comparison = await _weatherService.CompareCities(cities);
                return Ok(comparison);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao comparar cidades: {ex.Message}");
            }
        }

        [HttpGet("trends/{city}")]
        public async Task<ActionResult<WeatherTrend>> GetTrends(string city, [FromQuery] int days = 30)
        {
            try
            {
                var trends = await _weatherService.GetWeatherTrend(city, days);
                return Ok(trends);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao buscar tendências: {ex.Message}");
            }
        }

        [HttpGet("alerts/{city}")]
        public async Task<ActionResult<List<WeatherAlert>>> GetAlerts(string city)
        {
            try
            {
                var alerts = await _weatherService.GetWeatherAlerts(city);
                return Ok(alerts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao buscar alertas: {ex.Message}");
            }
        }
    }
}