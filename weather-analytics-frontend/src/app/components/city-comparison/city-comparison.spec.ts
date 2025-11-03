import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CityComparison } from './city-comparison';

describe('CityComparison', () => {
  let component: CityComparison;
  let fixture: ComponentFixture<CityComparison>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [CityComparison]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CityComparison);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
