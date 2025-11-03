import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { 
  WeatherData, 
  WeatherForecast, 
  HistoricalWeatherData, 
  CityComparison, 
  WeatherTrend, 
  WeatherAlert 
} from '../models/weather';

@Injectable({
  providedIn: 'root'
})
export class WeatherService {
  private apiUrl = 'https://localhost:7178/api/weather';

  constructor(private http: HttpClient) { }

  getCurrentWeather(city: string, country: string = ''): Observable<WeatherData> {
    const url = country 
      ? `${this.apiUrl}/current/${city}?country=${country}`
      : `${this.apiUrl}/current/${city}`;
    return this.http.get<WeatherData>(url);
  }

  getForecast(city: string, country: string = '', days: number = 5): Observable<WeatherForecast> {
    const url = `${this.apiUrl}/forecast/${city}?country=${country}&days=${days}`;
    return this.http.get<WeatherForecast>(url);
  }

  getHistoricalWeather(city: string, fromDate: Date, toDate: Date): Observable<HistoricalWeatherData[]> {
    const from = fromDate.toISOString().split('T')[0];
    const to = toDate.toISOString().split('T')[0];
    const url = `${this.apiUrl}/historical/${city}?fromDate=${from}&toDate=${to}`;
    return this.http.get<HistoricalWeatherData[]>(url);
  }

  compareCities(cities: string[]): Observable<CityComparison> {
    return this.http.post<CityComparison>(`${this.apiUrl}/compare`, cities);
  }

  getTrends(city: string, days: number = 30): Observable<WeatherTrend> {
    return this.http.get<WeatherTrend>(`${this.apiUrl}/trends/${city}?days=${days}`);
  }

  getAlerts(city: string): Observable<WeatherAlert[]> {
    return this.http.get<WeatherAlert[]>(`${this.apiUrl}/alerts/${city}`);
  }
}