import { Component, OnInit, Input, OnChanges, ViewChild, ElementRef, AfterViewInit, OnDestroy } from '@angular/core';
import { WeatherService } from '../../services/weather.service';
import Chart from 'chart.js/auto';

@Component({
  standalone: false,
  selector: 'app-weather-charts',
  templateUrl: './weather-charts.component.html',
  styleUrls: ['./weather-charts.component.scss']
})
export class WeatherChartsComponent implements OnInit, OnChanges, AfterViewInit, OnDestroy {
  @Input() city: string = 'São Paulo';
  
  @ViewChild('temperatureChart') temperatureChartRef!: ElementRef;
  @ViewChild('conditionsChart') conditionsChartRef!: ElementRef;
  @ViewChild('comparisonChart') comparisonChartRef!: ElementRef;
  @ViewChild('radarChart') radarChartRef!: ElementRef;
  
  charts: { [key: string]: any } = {};
  isLoading: boolean = false;
  private chartsInitialized = false;

  constructor(private weatherService: WeatherService) { }

  ngOnInit(): void {
    setTimeout(() => {
      this.loadChartsData();
    }, 300);
  }

  ngOnChanges(): void {
    if (this.chartsInitialized) {
      this.updateChartsData();
    }
  }

  ngAfterViewInit(): void {
    if (!this.chartsInitialized) {
      setTimeout(() => {
        this.loadChartsData();
      }, 500);
    }
  }

  loadChartsData(): void {
    if (this.isLoading || this.chartsInitialized) return;
    
    this.isLoading = true;
    
    if (!this.areCanvasElementsReady()) {
      console.warn('Canvas elements not ready, retrying...');
      setTimeout(() => this.loadChartsData(), 100);
      return;
    }

    this.createAllCharts();
    this.isLoading = false;
    this.chartsInitialized = true;
  }

  updateChartsData(): void {
    this.destroyCharts();
    this.chartsInitialized = false;
    setTimeout(() => {
      this.loadChartsData();
    }, 300);
  }

  areCanvasElementsReady(): boolean {
    return !!(
      this.temperatureChartRef?.nativeElement &&
      this.conditionsChartRef?.nativeElement &&
      this.comparisonChartRef?.nativeElement &&
      this.radarChartRef?.nativeElement
    );
  }

  createAllCharts(): void {
    this.createTemperatureChart();
    this.createConditionsChart();
    this.createComparisonChart();
    this.createRadarChart();
  }

  createTemperatureChart(): void {
    if (!this.temperatureChartRef?.nativeElement || this.charts['temperature']) {
      return;
    }

    try {
     const ctx = this.temperatureChartRef.nativeElement.getContext('2d');
      
      // Verificação de tipo segura
      const existingChart = this.charts['temperature'];
      if (existingChart && typeof existingChart.destroy === 'function') {
        existingChart.destroy();
      }
      
      this.charts['temperature'] = new Chart(ctx, {
        type: 'line',
        data: {
          labels: Array.from({length: 30}, (_, i) => {
            const date = new Date();
            date.setDate(date.getDate() - (29 - i));
            return date.toLocaleDateString('pt-BR', { day: '2-digit', month: '2-digit' });
          }),
          datasets: [{
            label: 'Temperatura (°C)',
            data: Array.from({length: 30}, () => 15 + Math.random() * 15),
            borderColor: '#ff6b6b',
            backgroundColor: 'rgba(255, 107, 107, 0.1)',
            borderWidth: 3,
            fill: true,
            tension: 0.4,
            pointBackgroundColor: '#ff6b6b',
            pointBorderColor: '#ffffff',
            pointBorderWidth: 2,
            pointRadius: 4
          }]
        },
        options: {
          responsive: true,
          maintainAspectRatio: false,
          plugins: {
            title: { 
              display: true, 
              text: `Tendência de Temperatura - ${this.city}`,
              font: { size: 14, weight: 'bold' }
            },
            legend: { 
              display: true,
              position: 'top'
            }
          },
          scales: {
            y: {
              beginAtZero: false,
              title: {
                display: true,
                text: 'Temperatura (°C)'
              },
              grid: {
                color: 'rgba(0, 0, 0, 0.1)'
              }
            },
            x: {
              title: {
                display: true,
                text: 'Data'
              },
              grid: {
                color: 'rgba(0, 0, 0, 0.05)'
              }
            }
          }
        }
      });
    } catch (error) {
      console.error('Error creating temperature chart:', error);
    }
  }

  createConditionsChart(): void {
    if (!this.conditionsChartRef?.nativeElement || this.charts['conditions']) {
      return;
    }

    try {
      const ctx = this.conditionsChartRef.nativeElement.getContext('2d');
      
      if (this.charts['conditions']) {
        this.charts['conditions'].destroy();
      }
      
      this.charts['conditions'] = new Chart(ctx, {
        type: 'doughnut',
        data: {
          labels: ['Ensolarado', 'Parcialmente Nublado', 'Nublado', 'Chuvoso', 'Tempestade'],
          datasets: [{
            data: [30, 25, 20, 15, 10],
            backgroundColor: [
              '#ffd93d',
              '#a5d8ff',
              '#6bceff',
              '#4d96ff',
              '#ff6b6b'
            ],
            borderWidth: 2,
            borderColor: '#ffffff'
          }]
        },
        options: {
          responsive: true,
          maintainAspectRatio: false,
          plugins: {
            title: { 
              display: true, 
              text: 'Distribuição de Condições Climáticas',
              font: { size: 14, weight: 'bold' }
            },
            legend: { 
              position: 'bottom',
              labels: {
                padding: 15,
                usePointStyle: true
              }
            }
          },
          cutout: '50%'
        }
      });
    } catch (error) {
      console.error('Error creating conditions chart:', error);
    }
  }

  createComparisonChart(): void {
    if (!this.comparisonChartRef?.nativeElement || this.charts['comparison']) {
      return;
    }

    try {
      const ctx = this.comparisonChartRef.nativeElement.getContext('2d');
      
      if (this.charts['comparison']) {
        this.charts['comparison'].destroy();
      }
      
      this.charts['comparison'] = new Chart(ctx, {
        type: 'bar',
        data: {
          labels: ['São Paulo', 'Rio de Janeiro', 'Brasília', 'Salvador', 'Fortaleza'],
          datasets: [
            {
              label: 'Temperatura Média (°C)',
              data: [22, 26, 24, 28, 30],
              backgroundColor: '#4d96ff',
              borderColor: '#4d96ff',
              borderWidth: 1,
              borderRadius: 4
            },
            {
              label: 'Umidade Média (%)',
              data: [75, 80, 65, 85, 78],
              backgroundColor: '#6bceff',
              borderColor: '#6bceff',
              borderWidth: 1,
              borderRadius: 4
            }
          ]
        },
        options: {
          responsive: true,
          maintainAspectRatio: false,
          plugins: {
            title: { 
              display: true, 
              text: 'Comparação entre Cidades',
              font: { size: 14, weight: 'bold' }
            }
          },
          scales: {
            y: {
              beginAtZero: true,
              title: {
                display: true,
                text: 'Valores'
              },
              grid: {
                color: 'rgba(0, 0, 0, 0.1)'
              }
            },
            x: {
              grid: {
                display: false
              }
            }
          }
        }
      });
    } catch (error) {
      console.error('Error creating comparison chart:', error);
    }
  }

  createRadarChart(): void {
    if (!this.radarChartRef?.nativeElement || this.charts['radar']) {
      return;
    }

    try {
      const ctx = this.radarChartRef.nativeElement.getContext('2d');
      
      if (this.charts['radar']) {
        this.charts['radar'].destroy();
      }
      
      this.charts['radar'] = new Chart(ctx, {
        type: 'radar',
        data: {
          labels: ['Temperatura', 'Umidade', 'Pressão', 'Velocidade do Vento', 'Precipitação', 'Índice UV'],
          datasets: [{
            label: 'Métricas Climáticas',
            data: [65, 75, 60, 80, 45, 70],
            backgroundColor: 'rgba(77, 150, 255, 0.2)',
            borderColor: '#4d96ff',
            borderWidth: 2,
            pointBackgroundColor: '#4d96ff',
            pointBorderColor: '#ffffff',
            pointBorderWidth: 2,
            pointRadius: 4
          }]
        },
        options: {
          responsive: true,
          maintainAspectRatio: false,
          plugins: {
            title: { 
              display: true, 
              text: 'Perfil Climático - Métricas Múltiplas',
              font: { size: 14, weight: 'bold' }
            }
          },
          scales: {
            r: {
              beginAtZero: true,
              max: 100,
              ticks: {
                stepSize: 20,
                backdropColor: 'transparent'
              },
              grid: {
                color: 'rgba(0, 0, 0, 0.1)'
              },
              angleLines: {
                color: 'rgba(0, 0, 0, 0.1)'
              },
              pointLabels: {
                font: {
                  size: 11
                }
              }
            }
          }
        }
      });
    } catch (error) {
      console.error('Error creating radar chart:', error);
    }
  }

  destroyCharts(): void {
    // Método seguro para destruir gráficos
    Object.keys(this.charts).forEach(key => {
      const chart = this.charts[key];
      if (chart && typeof chart.destroy === 'function') {
        try {
          chart.destroy();
        } catch (error) {
          console.warn(`Error destroying chart ${key}:`, error);
        }
      }
    });
    this.charts = {};
    this.chartsInitialized = false;
  }

  ngOnDestroy(): void {
    this.destroyCharts();
  }
}