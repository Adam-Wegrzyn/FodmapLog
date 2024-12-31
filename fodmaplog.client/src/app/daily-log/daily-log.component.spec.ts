import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DailyLogComponent } from './daily-log.component';

describe('DailyLogComponent', () => {
  let component: DailyLogComponent;
  let fixture: ComponentFixture<DailyLogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [DailyLogComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(DailyLogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
