namespace WeatherAnalytics.Core.Models
{
    public class WeatherData
    {
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public double Temperature { get; set; }
        public double FeelsLike { get; set; }
        public int Humidity { get; set; }
        public double Pressure { get; set; }
        public double WindSpeed { get; set; }
        public string WindDirection { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public DateTime Date { get; set; }
    }

    public class HistoricalWeatherData : WeatherData
    {
        public double? Precipitation { get; set; }
        public double? Visibility { get; set; }
        public double? UVIndex { get; set; }
    }

    public class WeatherForecast
    {
        public string City { get; set; } = string.Empty;
        public List<WeatherData> DailyForecasts { get; set; } = new();
        public List<WeatherData> HourlyForecasts { get; set; } = new();
    }

    public class CityComparison
    {
        public List<WeatherData> CitiesData { get; set; } = new();
        public DateTime ComparisonDate { get; set; }
    }

    public class WeatherTrend
    {
        public string City { get; set; } = string.Empty;
        public List<TrendData> TemperatureTrend { get; set; } = new();
        public List<TrendData> HumidityTrend { get; set; } = new();
        public List<TrendData> PressureTrend { get; set; } = new();
    }

    public class TrendData
    {
        public DateTime Date { get; set; }
        public double Value { get; set; }
    }

    public class WeatherAlert
    {
        public string City { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}