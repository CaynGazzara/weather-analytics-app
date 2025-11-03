export interface WeatherData {
  city: string;
  country: string;
  temperature: number;
  feelsLike: number;
  humidity: number;
  pressure: number;
  windSpeed: number;
  windDirection: string;
  description: string;
  icon: string;
  date: string;
}

export interface HistoricalWeatherData extends WeatherData {
  precipitation?: number;
  visibility?: number;
  uvIndex?: number;
}

export interface WeatherForecast {
  city: string;
  dailyForecasts: WeatherData[];
  hourlyForecasts: WeatherData[];
}

export interface CityComparison {
  citiesData: WeatherData[];
  comparisonDate: string;
}

export interface WeatherTrend {
  city: string;
  temperatureTrend: TrendData[];
  humidityTrend: TrendData[];
  pressureTrend: TrendData[];
}

export interface TrendData {
  date: string;
  value: number;
}

export interface WeatherAlert {
  city: string;
  type: string;
  severity: string;
  description: string;
  startTime: string;
  endTime: string;
}

export interface ChartConfig {
  type: string;
  data: any;
  options: any;
}