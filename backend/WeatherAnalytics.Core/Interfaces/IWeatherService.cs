using WeatherAnalytics.Core.Models;

namespace WeatherAnalytics.Core.Interfaces
{
    public interface IWeatherService
    {
        Task<WeatherData> GetCurrentWeather(string city, string country = "");
        Task<WeatherForecast> GetWeatherForecast(string city, string country = "", int days = 5);
        Task<List<HistoricalWeatherData>> GetHistoricalWeather(string city, DateTime fromDate, DateTime toDate);
        Task<CityComparison> CompareCities(List<string> cities);
        Task<WeatherTrend> GetWeatherTrend(string city, int days = 30);
        Task<List<WeatherAlert>> GetWeatherAlerts(string city);
    }
}