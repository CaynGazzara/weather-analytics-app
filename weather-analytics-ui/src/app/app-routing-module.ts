import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { DashboardComponent } from './components/dashboard/dashboard.component';
import { CityComparisonComponent } from './components/city-comparison/city-comparison.component';
import { HistoricalDataComponent } from './components/historical-data/historical-data.component';
import { WeatherAlertsComponent } from './components/weather-alerts/weather-alerts.component';

const routes: Routes = [
  { path: '', component: DashboardComponent },
  { path: 'dashboard', component: DashboardComponent },
  { path: 'comparison', component: CityComparisonComponent },
  { path: 'historical', component: HistoricalDataComponent },
  { path: 'alerts', component: WeatherAlertsComponent },
  { path: '**', redirectTo: '/dashboard' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }