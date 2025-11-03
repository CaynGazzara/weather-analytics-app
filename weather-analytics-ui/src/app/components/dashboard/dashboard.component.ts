import { Component, OnInit } from '@angular/core';
import { WeatherService } from '../../services/weather.service';
import { WeatherData, WeatherAlert, CityComparison } from '../../models/weather';

@Component({
  standalone: false,
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit {
  currentWeather: WeatherData | null = null;
  alerts: WeatherAlert[] = [];
  comparison: CityComparison | null = null;
  selectedCity: string = 'São Paulo';
  isLoading: boolean = false;

  popularCities = ['São Paulo', 'Rio de Janeiro', 'Brasília', 'Salvador', 'Fortaleza'];

  constructor(private weatherService: WeatherService) { }

  ngOnInit(): void {
    this.loadDashboardData();
  }

  loadDashboardData(): void {
    this.isLoading = true;
    
    this.weatherService.getCurrentWeather(this.selectedCity).subscribe({
      next: (data) => {
        this.currentWeather = data;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading weather data:', error);
        this.isLoading = false;
      }
    });

    this.weatherService.getAlerts(this.selectedCity).subscribe({
      next: (data) => {
        this.alerts = data;
      },
      error: (error) => {
        console.error('Error loading alerts:', error);
      }
    });

    this.weatherService.compareCities(this.popularCities.slice(0, 4)).subscribe({
      next: (data) => {
        this.comparison = data;
      },
      error: (error) => {
        console.error('Error loading comparison:', error);
      }
    });
  }

  onCityChange(): void {
    this.loadDashboardData();
  }

  getAlertSeverityClass(severity: string): string {
    switch (severity.toLowerCase()) {
      case 'alta': return 'alert-high';
      case 'média': return 'alert-medium';
      case 'baixa': return 'alert-low';
      default: return 'alert-low';
    }
  }

  getWeatherIconUrl(icon: string): string {
    return `https://openweathermap.org/img/w/${icon}.png`;
  }
}