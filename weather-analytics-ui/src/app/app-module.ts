import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { HttpClientModule } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';

import { AppRoutingModule } from './app-routing-module';
import { AppComponent } from './app.component';
import { DashboardComponent } from './components/dashboard/dashboard.component';
import { WeatherChartsComponent } from './components/weather-charts/weather-charts.component';
import { CityComparisonComponent } from './components/city-comparison/city-comparison.component';
import { WeatherAlertsComponent } from './components/weather-alerts/weather-alerts.component';
import { HistoricalDataComponent } from './components/historical-data/historical-data.component';


@NgModule({
  declarations: [
    AppComponent,
    DashboardComponent,
    WeatherChartsComponent,
    CityComparisonComponent,
    WeatherAlertsComponent,
    HistoricalDataComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    HttpClientModule,
    FormsModule,
    RouterModule,
  ],
  providers: [
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }