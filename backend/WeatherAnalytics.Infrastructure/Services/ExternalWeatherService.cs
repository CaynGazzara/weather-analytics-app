using Microsoft.Extensions.Configuration;
using System.Text.Json;
using WeatherAnalytics.Core.Interfaces;
using WeatherAnalytics.Core.Models;

namespace WeatherAnalytics.Infrastructure.Services
{
    public class ExternalWeatherService : IWeatherService
    {
        private readonly HttpClient _httpClient;
        private readonly string _openWeatherApiKey;
        private readonly string _weatherApiKey;

        public ExternalWeatherService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _openWeatherApiKey = configuration["WeatherProviders:OpenWeatherMap:ApiKey"] ?? "";
            _weatherApiKey = configuration["WeatherProviders:WeatherAPI:ApiKey"] ?? "";

            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        public async Task<WeatherData> GetCurrentWeather(string city, string country = "")
        {
            try
            {
                // Tenta OpenWeatherMap primeiro
                var response = await _httpClient.GetAsync(
                    $"https://api.openweathermap.org/data/2.5/weather?q={city},{country}&appid={_openWeatherApiKey}&units=metric&lang=pt_br");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var openWeatherData = JsonSerializer.Deserialize<OpenWeatherResponse>(content);

                    return new WeatherData
                    {
                        City = city,
                        Country = country,
                        Temperature = openWeatherData?.Main?.Temp ?? 0,
                        FeelsLike = openWeatherData?.Main?.FeelsLike ?? 0,
                        Humidity = openWeatherData?.Main?.Humidity ?? 0,
                        Pressure = openWeatherData?.Main?.Pressure ?? 0,
                        WindSpeed = openWeatherData?.Wind?.Speed ?? 0,
                        WindDirection = GetWindDirection(openWeatherData?.Wind?.Deg ?? 0),
                        Description = openWeatherData?.Weather?.FirstOrDefault()?.Description ?? "",
                        Icon = openWeatherData?.Weather?.FirstOrDefault()?.Icon ?? "",
                        Date = DateTime.UtcNow
                    };
                }
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Error fetching current weather: {ex.Message}");
            }

            // Fallback para WeatherAPI
            return await GetCurrentWeatherFromWeatherAPI(city, country);
        }

        public async Task<WeatherForecast> GetWeatherForecast(string city, string country = "", int days = 5)
        {
            try
            {
                var response = await _httpClient.GetAsync(
                    $"https://api.openweathermap.org/data/2.5/forecast?q={city},{country}&appid={_openWeatherApiKey}&units=metric&lang=pt_br");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var forecastData = JsonSerializer.Deserialize<OpenWeatherForecastResponse>(content);

                    var forecast = new WeatherForecast
                    {
                        City = city
                    };

                    // Processar previsões horárias
                    forecast.HourlyForecasts = forecastData?.List?.Select(item => new WeatherData
                    {
                        City = city,
                        Temperature = item.Main?.Temp ?? 0,
                        Humidity = item.Main?.Humidity ?? 0,
                        Pressure = item.Main?.Pressure ?? 0,
                        WindSpeed = item.Wind?.Speed ?? 0,
                        Description = item.Weather?.FirstOrDefault()?.Description ?? "",
                        Icon = item.Weather?.FirstOrDefault()?.Icon ?? "",
                        Date = DateTimeOffset.FromUnixTimeSeconds(item.Dt ?? 0).DateTime
                    }).ToList() ?? new List<WeatherData>();

                    // Agrupar por dia para previsões diárias
                    forecast.DailyForecasts = forecast.HourlyForecasts
                        .GroupBy(x => x.Date.Date)
                        .Select(g => g.OrderBy(x => x.Date).First())
                        .Take(days)
                        .ToList();

                    return forecast;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching forecast: {ex.Message}");
            }

            return new WeatherForecast { City = city };
        }

        public async Task<List<HistoricalWeatherData>> GetHistoricalWeather(string city, DateTime fromDate, DateTime toDate)
        {
            // Implementação simplificada - em produção, usar API de dados históricos
            var historicalData = new List<HistoricalWeatherData>();
            var random = new Random();

            for (var date = fromDate; date <= toDate; date = date.AddDays(1))
            {
                historicalData.Add(new HistoricalWeatherData
                {
                    City = city,
                    Date = date,
                    Temperature = random.Next(15, 35) + random.NextDouble(),
                    Humidity = random.Next(30, 90),
                    Pressure = random.Next(1000, 1030),
                    WindSpeed = random.Next(0, 15) + random.NextDouble(),
                    Precipitation = random.Next(0, 100) / 100.0,
                    Description = GetRandomWeatherDescription(random)
                });
            }

            return await Task.FromResult(historicalData);
        }

        public async Task<CityComparison> CompareCities(List<string> cities)
        {
            var comparison = new CityComparison
            {
                ComparisonDate = DateTime.UtcNow
            };

            foreach (var city in cities)
            {
                var weatherData = await GetCurrentWeather(city);
                comparison.CitiesData.Add(weatherData);
            }

            return comparison;
        }

        public async Task<WeatherTrend> GetWeatherTrend(string city, int days = 30)
        {
            var endDate = DateTime.UtcNow;
            var startDate = endDate.AddDays(-days);
            
            var historicalData = await GetHistoricalWeather(city, startDate, endDate);

            return new WeatherTrend
            {
                City = city,
                TemperatureTrend = historicalData.Select(d => new TrendData 
                { 
                    Date = d.Date, 
                    Value = d.Temperature 
                }).ToList(),
                HumidityTrend = historicalData.Select(d => new TrendData 
                { 
                    Date = d.Date, 
                    Value = d.Humidity 
                }).ToList(),
                PressureTrend = historicalData.Select(d => new TrendData 
                { 
                    Date = d.Date, 
                    Value = d.Pressure 
                }).ToList()
            };
        }

        public async Task<List<WeatherAlert>> GetWeatherAlerts(string city)
        {
            var alerts = new List<WeatherAlert>();
            var random = new Random();
            
            // Simular alertas baseados nas condições atuais
            var currentWeather = await GetCurrentWeather(city);
            
            if (currentWeather.Temperature > 35)
            {
                alerts.Add(new WeatherAlert
                {
                    City = city,
                    Type = "Calor Extremo",
                    Severity = "Alta",
                    Description = "Temperatura muito alta esperada",
                    StartTime = DateTime.UtcNow,
                    EndTime = DateTime.UtcNow.AddHours(24)
                });
            }

            if (currentWeather.WindSpeed > 20)
            {
                alerts.Add(new WeatherAlert
                {
                    City = city,
                    Type = "Ventos Fortes",
                    Severity = "Média",
                    Description = "Ventos fortes esperados",
                    StartTime = DateTime.UtcNow,
                    EndTime = DateTime.UtcNow.AddHours(12)
                });
            }

            return alerts;
        }

        private async Task<WeatherData> GetCurrentWeatherFromWeatherAPI(string city, string country)
        {
            try
            {
                var response = await _httpClient.GetAsync(
                    $"http://api.weatherapi.com/v1/current.json?key={_weatherApiKey}&q={city},{country}&lang=pt");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var weatherApiData = JsonSerializer.Deserialize<WeatherAPIResponse>(content);

                    return new WeatherData
                    {
                        City = city,
                        Country = country,
                        Temperature = weatherApiData?.Current?.TempC ?? 0,
                        FeelsLike = weatherApiData?.Current?.FeelsLikeC ?? 0,
                        Humidity = weatherApiData?.Current?.Humidity ?? 0,
                        Pressure = weatherApiData?.Current?.PressureMb ?? 0,
                        WindSpeed = weatherApiData?.Current?.WindKph / 3.6 ?? 0, // Converter para m/s
                        WindDirection = weatherApiData?.Current?.WindDir ?? "",
                        Description = weatherApiData?.Current?.Condition?.Text ?? "",
                        Icon = $"https:{weatherApiData?.Current?.Condition?.Icon}",
                        Date = DateTime.UtcNow
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching from WeatherAPI: {ex.Message}");
            }

            // Fallback para dados mockados
            return GetMockWeatherData(city, country);
        }

        private WeatherData GetMockWeatherData(string city, string country)
        {
            var random = new Random();
            return new WeatherData
            {
                City = city,
                Country = country,
                Temperature = random.Next(15, 35) + random.NextDouble(),
                FeelsLike = random.Next(15, 35) + random.NextDouble(),
                Humidity = random.Next(30, 90),
                Pressure = random.Next(1000, 1030),
                WindSpeed = random.Next(0, 15) + random.NextDouble(),
                WindDirection = GetWindDirection(random.Next(0, 360)),
                Description = GetRandomWeatherDescription(random),
                Icon = "01d",
                Date = DateTime.UtcNow
            };
        }

        private string GetWindDirection(double degrees)
        {
            return degrees switch
            {
                >= 337.5 or < 22.5 => "N",
                >= 22.5 and < 67.5 => "NE",
                >= 67.5 and < 112.5 => "L",
                >= 112.5 and < 157.5 => "SE",
                >= 157.5 and < 202.5 => "S",
                >= 202.5 and < 247.5 => "SO",
                >= 247.5 and < 292.5 => "O",
                >= 292.5 and < 337.5 => "NO",
                _ => "N"
            };
        }

        private string GetRandomWeatherDescription(Random random)
        {
            var descriptions = new[]
            {
                "Céu limpo", "Parcialmente nublado", "Nublado", "Chuva leve", 
                "Chuva moderada", "Chuva forte", "Tempestade", "Neve", 
                "Neblina", "Ventania"
            };
            return descriptions[random.Next(descriptions.Length)];
        }
    }

    // Classes para desserialização das APIs externas
    public class OpenWeatherResponse
    {
        public MainData? Main { get; set; }
        public WindData? Wind { get; set; }
        public List<WeatherDescription>? Weather { get; set; }
    }

    public class MainData
    {
        public double Temp { get; set; }
        public double FeelsLike { get; set; }
        public int Humidity { get; set; }
        public double Pressure { get; set; }
    }

    public class WindData
    {
        public double Speed { get; set; }
        public double Deg { get; set; }
    }

    public class WeatherDescription
    {
        public string? Description { get; set; }
        public string? Icon { get; set; }
    }

    public class OpenWeatherForecastResponse
    {
        public List<ForecastItem>? List { get; set; }
    }

    public class ForecastItem
    {
        public MainData? Main { get; set; }
        public WindData? Wind { get; set; }
        public List<WeatherDescription>? Weather { get; set; }
        public long? Dt { get; set; }
    }

    public class WeatherAPIResponse
    {
        public CurrentData? Current { get; set; }
    }

    public class CurrentData
    {
        public double TempC { get; set; }
        public double FeelsLikeC { get; set; }
        public int Humidity { get; set; }
        public double PressureMb { get; set; }
        public double WindKph { get; set; }
        public string? WindDir { get; set; }
        public ConditionData? Condition { get; set; }
    }

    public class ConditionData
    {
        public string? Text { get; set; }
        public string? Icon { get; set; }
    }
}