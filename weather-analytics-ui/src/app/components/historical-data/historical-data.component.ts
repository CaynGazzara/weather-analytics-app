import { Component, OnInit } from '@angular/core';
import { WeatherService } from '../../services/weather.service';
import { HistoricalWeatherData, WeatherTrend } from '../../models/weather';

@Component({
  standalone: false,
  selector: 'app-historical-data',
  templateUrl: './historical-data.component.html',
  styleUrls: ['./historical-data.component.scss']
})
export class HistoricalDataComponent implements OnInit {
  historicalData: HistoricalWeatherData[] = [];
  trends: WeatherTrend | null = null;
  selectedCity: string = 'São Paulo';
  startDate: string;
  endDate: string;
  isLoading: boolean = false;
  showTrends: boolean = true;

  cities = ['São Paulo', 'Rio de Janeiro', 'Brasília', 'Salvador', 'Fortaleza'];

  constructor(private weatherService: WeatherService) {
    // Definir datas padrão (últimos 30 dias)
    const end = new Date();
    const start = new Date();
    start.setDate(start.getDate() - 30);
    
    this.startDate = start.toISOString().split('T')[0];
    this.endDate = end.toISOString().split('T')[0];
  }

  ngOnInit(): void {
    this.loadHistoricalData();
  }

  loadHistoricalData(): void {
    this.isLoading = true;
    const startDate = new Date(this.startDate);
    const endDate = new Date(this.endDate);

    this.weatherService.getHistoricalWeather(this.selectedCity, startDate, endDate)
      .subscribe({
        next: (data) => {
          this.historicalData = data;
          this.isLoading = false;
        },
        error: (error) => {
          console.error('Error loading historical data:', error);
          this.isLoading = false;
        }
      });

    if (this.showTrends) {
      const days = Math.ceil((endDate.getTime() - startDate.getTime()) / (1000 * 60 * 60 * 24));
      this.weatherService.getTrends(this.selectedCity, Math.min(days, 365))
        .subscribe({
          next: (trends) => {
            this.trends = trends;
          },
          error: (error) => {
            console.error('Error loading trends:', error);
          }
        });
    }
  }

  onFiltersChange(): void {
    this.loadHistoricalData();
  }

  getStats(): any {
    if (!this.historicalData.length) return null;

    const temps = this.historicalData.map(d => d.temperature);
    const humidities = this.historicalData.map(d => d.humidity);
    const pressures = this.historicalData.map(d => d.pressure);

    return {
      avgTemp: temps.reduce((a, b) => a + b, 0) / temps.length,
      maxTemp: Math.max(...temps),
      minTemp: Math.min(...temps),
      avgHumidity: humidities.reduce((a, b) => a + b, 0) / humidities.length,
      avgPressure: pressures.reduce((a, b) => a + b, 0) / pressures.length
    };
  }

  exportToCSV(): void {
    if (!this.historicalData.length) return;

    const headers = ['Data', 'Temperatura (°C)', 'Umidade (%)', 'Pressão (hPa)', 'Vento (m/s)', 'Condição'];
    const csvData = this.historicalData.map(item => [
      new Date(item.date).toLocaleDateString('pt-BR'),
      item.temperature.toFixed(1),
      item.humidity,
      item.pressure,
      item.windSpeed.toFixed(1),
      item.description
    ]);

    const csvContent = [headers, ...csvData]
      .map(row => row.map(field => `"${field}"`).join(','))
      .join('\n');

    const blob = new Blob([csvContent], { type: 'text/csv' });
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = `dados_climaticos_${this.selectedCity}_${this.startDate}_a_${this.endDate}.csv`;
    link.click();
    window.URL.revokeObjectURL(url);
  }
}