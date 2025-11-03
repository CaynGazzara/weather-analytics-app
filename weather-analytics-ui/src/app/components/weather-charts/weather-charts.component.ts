import { Component, OnInit, Input, OnChanges, ViewChild, ElementRef, AfterViewInit } from '@angular/core';
import { WeatherService } from '../../services/weather.service';
declare var Chart: any; // Usar Chart.js globalmente

@Component({
  standalone: false,
  selector: 'app-weather-charts',
  templateUrl: './weather-charts.component.html',
  styleUrls: ['./weather-charts.component.scss']
})
export class WeatherChartsComponent implements OnInit, OnChanges, AfterViewInit {
  @Input() city: string = 'São Paulo';
  
  @ViewChild('temperatureChart') temperatureChartRef!: ElementRef;
  @ViewChild('conditionsChart') conditionsChartRef!: ElementRef;
  @ViewChild('comparisonChart') comparisonChartRef!: ElementRef;
  @ViewChild('radarChart') radarChartRef!: ElementRef;
  
  charts: any = {};
  isLoading: boolean = false;

  constructor(private weatherService: WeatherService) { }

  ngOnInit(): void {
    this.loadChartsData();
  }

  ngOnChanges(): void {
    this.loadChartsData();
  }

  ngAfterViewInit(): void {
    // Os gráficos serão criados após o carregamento dos dados
  }

  loadChartsData(): void {
    this.isLoading = true;
    
    // Usar dados mock por enquanto
    this.createMockCharts();
    this.isLoading = false;
  }

  createMockCharts(): void {
    this.destroyCharts();

    setTimeout(() => {
      this.createTemperatureChart();
      this.createConditionsChart();
      this.createComparisonChart();
      this.createRadarChart();
    }, 100);
  }

  createTemperatureChart(): void {
    if (!this.temperatureChartRef?.nativeElement) return;

    const ctx = this.temperatureChartRef.nativeElement.getContext('2d');
    
    this.charts.temperature = new Chart(ctx, {
      type: 'line',
      data: {
        labels: Array.from({length: 30}, (_, i) => {
          const date = new Date();
          date.setDate(date.getDate() - (29 - i));
          return date.toLocaleDateString('pt-BR');
        }),
        datasets: [{
          label: 'Temperatura (°C)',
          data: Array.from({length: 30}, () => 20 + Math.random() * 10),
          borderColor: '#ff6b6b',
          backgroundColor: 'rgba(255, 107, 107, 0.1)',
          borderWidth: 2,
          fill: true,
          tension: 0.4
        }]
      },
      options: {
        responsive: true,
        plugins: {
          title: { display: true, text: 'Tendência de Temperatura (30 dias)' },
          legend: { display: true }
        }
      }
    });
  }

  createConditionsChart(): void {
    if (!this.conditionsChartRef?.nativeElement) return;

    const ctx = this.conditionsChartRef.nativeElement.getContext('2d');
    
    this.charts.conditions = new Chart(ctx, {
      type: 'doughnut',
      data: {
        labels: ['Ensolarado', 'Nublado', 'Chuvoso', 'Tempestade', 'Neblina'],
        datasets: [{
          data: [35, 25, 20, 10, 10],
          backgroundColor: ['#ffd93d', '#6bceff', '#4d96ff', '#ff6b6b', '#95adbe'],
          borderWidth: 2
        }]
      },
      options: {
        responsive: true,
        plugins: {
          title: { display: true, text: 'Distribuição de Condições Climáticas' },
          legend: { position: 'bottom' }
        }
      }
    });
  }

  createComparisonChart(): void {
    if (!this.comparisonChartRef?.nativeElement) return;

    const ctx = this.comparisonChartRef.nativeElement.getContext('2d');
    
    this.charts.comparison = new Chart(ctx, {
      type: 'bar',
      data: {
        labels: ['São Paulo', 'Rio de Janeiro', 'Brasília', 'Salvador'],
        datasets: [
          {
            label: 'Temperatura Média (°C)',
            data: [22, 26, 24, 28],
            backgroundColor: '#4d96ff'
          },
          {
            label: 'Umidade Média (%)',
            data: [75, 80, 65, 85],
            backgroundColor: '#6bceff'
          }
        ]
      },
      options: {
        responsive: true,
        plugins: {
          title: { display: true, text: 'Comparação entre Cidades' }
        }
      }
    });
  }

  createRadarChart(): void {
    if (!this.radarChartRef?.nativeElement) return;

    const ctx = this.radarChartRef.nativeElement.getContext('2d');
    
    this.charts.radar = new Chart(ctx, {
      type: 'radar',
      data: {
        labels: ['Temperatura', 'Umidade', 'Pressão', 'Vento', 'Precipitação', 'UV Index'],
        datasets: [{
          label: 'Métricas Climáticas',
          data: [65, 75, 60, 80, 45, 70],
          backgroundColor: 'rgba(77, 150, 255, 0.2)',
          borderColor: '#4d96ff',
          borderWidth: 2
        }]
      },
      options: {
        responsive: true,
        plugins: {
          title: { display: true, text: 'Perfil Climático' }
        },
        scales: {
          r: {
            beginAtZero: true,
            max: 100
          }
        }
      }
    });
  }

  destroyCharts(): void {
    Object.values(this.charts).forEach((chart: any) => {
      if (chart && typeof chart.destroy === 'function') {
        chart.destroy();
      }
    });
    this.charts = {};
  }

  ngOnDestroy(): void {
    this.destroyCharts();
  }
}