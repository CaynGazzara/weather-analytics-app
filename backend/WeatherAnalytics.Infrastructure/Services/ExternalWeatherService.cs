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
        private readonly Random _random = new Random();

        public ExternalWeatherService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _openWeatherApiKey = configuration["WeatherProviders:OpenWeatherMap:ApiKey"] ?? "";
            _weatherApiKey = configuration["WeatherProviders:WeatherAPI:ApiKey"] ?? "";
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        public async Task<WeatherData> GetCurrentWeather(string city, string country = "")
        {
            Console.WriteLine($"🔍 Buscando dados climáticos para: {city}, {country}");

            try
            {
                // Tenta WeatherAPI primeiro
                var weatherApiData = await GetCurrentWeatherFromWeatherAPI(city, country);

                // ⚠️ CORREÇÃO: Verificação mais robusta
                if (weatherApiData != null && weatherApiData.Temperature != 0 && !string.IsNullOrEmpty(weatherApiData.Description))
                {
                    Console.WriteLine($"✅ Retornando dados REAIS da WeatherAPI: {weatherApiData.Temperature}°C");
                    return weatherApiData;
                }

                // Fallback para OpenWeatherMap
                Console.WriteLine($"🔄 WeatherAPI não retornou dados válidos, tentando OpenWeatherMap...");
                var openWeatherData = await GetCurrentWeatherFromOpenWeather(city, country);

                if (openWeatherData != null && openWeatherData.Temperature != 0)
                {
                    Console.WriteLine($"✅ Retornando dados do OpenWeatherMap: {openWeatherData.Temperature}°C");
                    return openWeatherData;
                }

                // Último fallback - SEMPRE retorna algo
                Console.WriteLine($"⚠️ Ambas APIs falharam, usando dados mock");
                var mockData = GetMockWeatherData(city, country);
                Console.WriteLine($"✅ Retornando dados MOCK: {mockData.Temperature}°C");
                return mockData;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erro geral em GetCurrentWeather: {ex.Message}");
                // ⚠️ IMPORTANTE: Sempre retorna dados mock em caso de erro
                var mockData = GetMockWeatherData(city, country);
                Console.WriteLine($"✅ Retornando dados MOCK após erro: {mockData.Temperature}°C");
                return mockData;
            }
        }

        public async Task<WeatherForecast> GetWeatherForecast(string city, string country = "", int days = 5)
        {
            try
            {
                var weatherApiUrl = $"http://api.weatherapi.com/v1/forecast.json?key={_weatherApiKey}&q={city},{country}&days={days}&lang=pt";
                Console.WriteLine($"🌤️ Tentando WeatherAPI Forecast: {weatherApiUrl}");

                var response = await _httpClient.GetAsync(weatherApiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var forecastData = JsonSerializer.Deserialize<WeatherAPIForecastResponse>(content);

                    if (forecastData?.Forecast?.Forecastday != null)
                    {
                        var forecast = new WeatherForecast { City = city };

                        foreach (var day in forecastData.Forecast.Forecastday)
                        {
                            forecast.DailyForecasts.Add(new WeatherData
                            {
                                City = city,
                                Temperature = day.Day?.AvgtempC ?? 0,
                                Humidity = (int)(day.Day?.Avghumidity ?? 0),
                                Description = day.Day?.Condition?.Text ?? "",
                                Icon = $"https:{day.Day?.Condition?.Icon}",
                                Date = DateTime.Parse(day.Date ?? DateTime.UtcNow.ToString())
                            });
                        }

                        return forecast;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erro no WeatherAPI Forecast: {ex.Message}");
            }

            // Fallback para dados mock
            return await GetMockWeatherForecast(city, country, days);
        }

        public async Task<List<HistoricalWeatherData>> GetHistoricalWeather(string city, DateTime fromDate, DateTime toDate)
        {
            // WeatherAPI oferece dados históricos, mas requer plano pago
            // Vamos usar dados mock por enquanto
            return await GetMockHistoricalWeather(city, fromDate, toDate);
        }

        public async Task<CityComparison> CompareCities(List<string> cities)
        {
            Console.WriteLine($"🔍 Comparando cidades: {string.Join(", ", cities)}");

            var comparison = new CityComparison
            {
                ComparisonDate = DateTime.UtcNow
            };

            foreach (var city in cities)
            {
                try
                {
                    var weatherData = await GetCurrentWeather(city);
                    comparison.CitiesData.Add(weatherData);
                    Console.WriteLine($"✅ Dados obtidos para {city}: {weatherData.Temperature}°C");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Erro ao obter dados para {city}: {ex.Message}");
                    // Adiciona dados mock em caso de erro para não quebrar a comparação
                    var mockData = GetMockWeatherData(city, "");
                    comparison.CitiesData.Add(mockData);
                }
            }

            Console.WriteLine($"✅ Comparação concluída com {comparison.CitiesData.Count} cidades");
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

            try
            {
                // Busca dados atuais para verificar condições de alerta
                var currentWeather = await GetCurrentWeather(city);

                // ⚠️ CORREÇÃO: Verificar se currentWeather não é null
                if (currentWeather == null)
                {
                    Console.WriteLine($"⚠️ Não foi possível obter dados para alertas em {city}");
                    return alerts; // Retorna lista vazia
                }

                // Alertas baseados em condições reais
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

                if (currentWeather.Humidity > 90)
                {
                    alerts.Add(new WeatherAlert
                    {
                        City = city,
                        Type = "Umidade Elevada",
                        Severity = "Baixa",
                        Description = "Umidade muito alta",
                        StartTime = DateTime.UtcNow,
                        EndTime = DateTime.UtcNow.AddHours(6)
                    });
                }

                Console.WriteLine($"✅ {alerts.Count} alertas encontrados para {city}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erro ao buscar alertas: {ex.Message}");
                Console.WriteLine($"❌ StackTrace: {ex.StackTrace}");
            }

            return alerts;
        }

        // Métodos auxiliares privados
        private async Task<WeatherData> GetCurrentWeatherFromWeatherAPI(string city, string country)
        {
            try
            {
                var weatherApiUrl = $"http://api.weatherapi.com/v1/current.json?key={_weatherApiKey}&q={city},{country}&lang=pt";
                Console.WriteLine($"🌤️ Tentando WeatherAPI: {weatherApiUrl}");

                var response = await _httpClient.GetAsync(weatherApiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"✅ WeatherAPI respondeu com sucesso!");

                    var weatherApiData = JsonSerializer.Deserialize<WeatherAPIResponse>(content);

                    if (weatherApiData?.Current != null)
                    {
                        var weatherData = new WeatherData
                        {
                            City = city,
                            Country = country,
                            Temperature = weatherApiData.Current.TempC,
                            FeelsLike = weatherApiData.Current.FeelsLikeC,
                            Humidity = weatherApiData.Current.Humidity,
                            Pressure = weatherApiData.Current.PressureMb,
                            WindSpeed = weatherApiData.Current.WindKph / 3.6, // Converter para m/s
                            WindDirection = weatherApiData.Current.WindDir ?? "",
                            Description = weatherApiData.Current.Condition?.Text ?? "",
                            Icon = $"https:{weatherApiData.Current.Condition?.Icon}",
                            Date = DateTime.UtcNow
                        };

                        Console.WriteLine($"✅ Dados REAIS do WeatherAPI: {weatherData.Temperature}°C - {weatherData.Description}");
                        return weatherData;
                    }
                }
                else
                {
                    Console.WriteLine($"❌ WeatherAPI falhou: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erro no WeatherAPI: {ex.Message}");
            }

            return null;
        }

        private async Task<WeatherData> GetCurrentWeatherFromOpenWeather(string city, string country)
        {
            try
            {
                var openWeatherUrl = $"https://api.openweathermap.org/data/2.5/weather?q={city},{country}&appid={_openWeatherApiKey}&units=metric&lang=pt_br";
                Console.WriteLine($"🌤️ Tentando OpenWeatherMap: {openWeatherUrl}");

                var response = await _httpClient.GetAsync(openWeatherUrl);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var openWeatherData = JsonSerializer.Deserialize<OpenWeatherResponse>(content);

                    if (openWeatherData?.Main != null)
                    {
                        return new WeatherData
                        {
                            City = city,
                            Country = country,
                            Temperature = openWeatherData.Main.Temp,
                            FeelsLike = openWeatherData.Main.FeelsLike,
                            Humidity = openWeatherData.Main.Humidity,
                            Pressure = openWeatherData.Main.Pressure,
                            WindSpeed = openWeatherData.Wind?.Speed ?? 0,
                            WindDirection = GetWindDirection(openWeatherData.Wind?.Deg ?? 0),
                            Description = openWeatherData.Weather?.FirstOrDefault()?.Description ?? "",
                            Icon = openWeatherData.Weather?.FirstOrDefault()?.Icon ?? "",
                            Date = DateTime.UtcNow
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erro no OpenWeatherMap: {ex.Message}");
            }

            // Último fallback para dados mock
            Console.WriteLine($"⚠️ Ambas APIs falharam, usando dados mock");
            return GetMockWeatherData(city, country);
        }

        private async Task<WeatherForecast> GetMockWeatherForecast(string city, string country, int days)
        {
            await Task.Delay(100); // Simular latência

            var forecast = new WeatherForecast { City = city };
            var baseDate = DateTime.UtcNow;

            for (int i = 0; i < days; i++)
            {
                var (minTemp, maxTemp) = GetTemperatureRange(city);
                var temperature = Math.Round(minTemp + (_random.NextDouble() * (maxTemp - minTemp)), 1);

                forecast.DailyForecasts.Add(new WeatherData
                {
                    City = city,
                    Temperature = temperature,
                    Humidity = _random.Next(60, 85),
                    Pressure = _random.Next(1010, 1020),
                    WindSpeed = Math.Round(1 + (_random.NextDouble() * 10), 1),
                    Description = GetRandomWeatherDescription(),
                    Icon = GetRandomWeatherIcon(),
                    Date = baseDate.AddDays(i)
                });
            }

            return forecast;
        }

        private async Task<List<HistoricalWeatherData>> GetMockHistoricalWeather(string city, DateTime fromDate, DateTime toDate)
        {
            await Task.Delay(100);

            var historicalData = new List<HistoricalWeatherData>();
            var (minTemp, maxTemp) = GetTemperatureRange(city);

            for (var date = fromDate; date <= toDate; date = date.AddDays(1))
            {
                var temperature = Math.Round(minTemp + (_random.NextDouble() * (maxTemp - minTemp)), 1);

                historicalData.Add(new HistoricalWeatherData
                {
                    City = city,
                    Date = date,
                    Temperature = temperature,
                    Humidity = _random.Next(55, 90),
                    Pressure = _random.Next(1008, 1022),
                    WindSpeed = Math.Round(1 + (_random.NextDouble() * 12), 1),
                    Precipitation = _random.Next(0, 100) / 100.0,
                    Description = GetRandomWeatherDescription()
                });
            }

            return historicalData;
        }

        private (double min, double max) GetTemperatureRange(string city)
        {
            return city.ToLower() switch
            {
                "são paulo" or "sao paulo" => (15.0, 28.0),
                "rio de janeiro" => (20.0, 32.0),
                "brasília" => (18.0, 30.0),
                "salvador" => (22.0, 30.0),
                "fortaleza" => (24.0, 32.0),
                "porto alegre" => (12.0, 25.0),
                "belo horizonte" => (16.0, 28.0),
                "manaus" => (24.0, 34.0),
                _ => (15.0, 30.0)
            };
        }

        private WeatherData GetMockWeatherData(string city, string country)
        {
            var (minTemp, maxTemp) = GetTemperatureRange(city);
            var temperature = Math.Round(minTemp + (_random.NextDouble() * (maxTemp - minTemp)), 1);
            var feelsLike = Math.Round(temperature + (_random.NextDouble() * 2 - 1), 1);

            return new WeatherData
            {
                City = city,
                Country = country,
                Temperature = temperature,
                FeelsLike = feelsLike,
                Humidity = _random.Next(50, 90),
                Pressure = _random.Next(1010, 1020),
                WindSpeed = Math.Round(1 + (_random.NextDouble() * 15), 1),
                WindDirection = GetWindDirection(_random.Next(0, 360)),
                Description = GetRandomWeatherDescription(),
                Icon = GetRandomWeatherIcon(),
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

        private string GetRandomWeatherDescription()
        {
            var descriptions = new[]
            {
                "Céu limpo", "Parcialmente nublado", "Nublado", "Chuva leve",
                "Chuva moderada", "Chuva forte", "Tempestade", "Neblina", "Ventania"
            };
            return descriptions[_random.Next(descriptions.Length)];
        }

        private string GetRandomWeatherIcon()
        {
            var icons = new[] { "01d", "02d", "03d", "04d", "09d", "10d", "11d", "50d" };
            return icons[_random.Next(icons.Length)];
        }
    }

    // Classes para desserialização
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

    public class WeatherAPIResponse
    {
        public CurrentData? Current { get; set; }
    }

    public class WeatherAPIForecastResponse
    {
        public ForecastData? Forecast { get; set; }
    }

    public class ForecastData
    {
        public List<ForecastDay>? Forecastday { get; set; }
    }

    public class ForecastDay
    {
        public string? Date { get; set; }
        public DayData? Day { get; set; }
    }

    public class DayData
    {
        public double? AvgtempC { get; set; }
        public double? Avghumidity { get; set; }
        public ConditionData? Condition { get; set; }
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