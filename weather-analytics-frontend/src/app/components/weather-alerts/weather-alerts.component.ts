import { Component, Input, OnInit } from '@angular/core';
import { WeatherService } from '../../services/weather.service';
import { WeatherAlert } from '../../models/weather';

@Component({
  standalone: false,
  selector: 'app-weather-alerts',
  templateUrl: './weather-alerts.component.html',
  styleUrls: ['./weather-alerts.component.scss']
})
export class WeatherAlertsComponent implements OnInit {
  @Input() city: string = 'São Paulo';
  alerts: WeatherAlert[] = [];
  isLoading: boolean = false;

  constructor(private weatherService: WeatherService) { }

  ngOnInit(): void {
    this.loadAlerts();
  }

  ngOnChanges(): void {
    this.loadAlerts();
  }

  loadAlerts(): void {
    this.isLoading = true;
    this.weatherService.getAlerts(this.city).subscribe({
      next: (data) => {
        this.alerts = data;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading alerts:', error);
        this.alerts = [];
        this.isLoading = false;
      }
    });
  }

  getAlertSeverityClass(severity: string): string {
    switch (severity.toLowerCase()) {
      case 'alta': return 'alert-high';
      case 'média': return 'alert-medium';
      case 'baixa': return 'alert-low';
      default: return 'alert-low';
    }
  }

  getAlertIcon(type: string): string {
    switch (type.toLowerCase()) {
      case 'tempestade': return '⛈️';
      case 'ventos fortes': return '💨';
      case 'calor extremo': return '🔥';
      case 'geada': return '❄️';
      case 'chuva intensa': return '🌧️';
      case 'nevoeiro': return '🌫️';
      default: return '⚠️';
    }
  }
}