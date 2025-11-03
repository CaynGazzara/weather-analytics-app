import { Component, OnInit } from '@angular/core';
import { WeatherService } from '../../services/weather.service';
import { CityComparison, WeatherData } from '../../models/weather';

@Component({
  standalone: false,
  selector: 'app-city-comparison',
  templateUrl: './city-comparison.component.html',
  styleUrls: ['./city-comparison.component.scss']
})
export class CityComparisonComponent implements OnInit {
  comparison: CityComparison | null = null;
  selectedCities: string[] = ['São Paulo', 'Rio de Janeiro', 'Brasília'];
  availableCities: string[] = [
    'São Paulo', 'Rio de Janeiro', 'Brasília', 'Salvador', 'Fortaleza',
    'Belo Horizonte', 'Manaus', 'Curitiba', 'Recife', 'Porto Alegre'
  ];
  isLoading: boolean = false;

  constructor(private weatherService: WeatherService) { }

  ngOnInit(): void {
    this.compareCities();
  }

  compareCities(): void {
    if (this.selectedCities.length === 0) return;

    this.isLoading = true;
    this.weatherService.compareCities(this.selectedCities).subscribe({
      next: (data) => {
        this.comparison = data;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error comparing cities:', error);
        this.isLoading = false;
      }
    });
  }

  onCitySelectionChange(city: string, event: any): void {
    if (event.target.checked) {
      if (!this.selectedCities.includes(city)) {
        this.selectedCities.push(city);
      }
    } else {
      this.selectedCities = this.selectedCities.filter(c => c !== city);
    }
    this.compareCities();
  }

  isCitySelected(city: string): boolean {
    return this.selectedCities.includes(city);
  }

  getTemperatureColor(temp: number): string {
    if (temp < 15) return 'temp-cold';
    if (temp < 25) return 'temp-mild';
    return 'temp-warm';
  }

  getHumidityColor(humidity: number): string {
    if (humidity < 40) return 'humidity-low';
    if (humidity < 70) return 'humidity-medium';
    return 'humidity-high';
  }
  getHottestCity(): WeatherData | null {
    if (!this.comparison?.citiesData.length) return null;
    return this.comparison.citiesData.reduce((hottest, current) =>
      current.temperature > hottest.temperature ? current : hottest
    );
  }

  getColdestCity(): WeatherData | null {
    if (!this.comparison?.citiesData.length) return null;
    return this.comparison.citiesData.reduce((coldest, current) =>
      current.temperature < coldest.temperature ? current : coldest
    );
  }

  getMostHumidCity(): WeatherData | null {
    if (!this.comparison?.citiesData.length) return null;
    return this.comparison.citiesData.reduce((mostHumid, current) =>
      current.humidity > mostHumid.humidity ? current : mostHumid
    );
  }

  getWindiestCity(): WeatherData | null {
    if (!this.comparison?.citiesData.length) return null;
    return this.comparison.citiesData.reduce((windiest, current) =>
      current.windSpeed > windiest.windSpeed ? current : windiest
    );
  }
}