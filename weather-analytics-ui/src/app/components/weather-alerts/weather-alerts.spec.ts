import { ComponentFixture, TestBed } from '@angular/core/testing';

import { WeatherAlerts } from './weather-alerts';

describe('WeatherAlerts', () => {
  let component: WeatherAlerts;
  let fixture: ComponentFixture<WeatherAlerts>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [WeatherAlerts]
    })
    .compileComponents();

    fixture = TestBed.createComponent(WeatherAlerts);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
