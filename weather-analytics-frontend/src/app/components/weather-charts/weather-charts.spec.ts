import { ComponentFixture, TestBed } from '@angular/core/testing';

import { WeatherCharts } from './weather-charts';

describe('WeatherCharts', () => {
  let component: WeatherCharts;
  let fixture: ComponentFixture<WeatherCharts>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [WeatherCharts]
    })
    .compileComponents();

    fixture = TestBed.createComponent(WeatherCharts);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
