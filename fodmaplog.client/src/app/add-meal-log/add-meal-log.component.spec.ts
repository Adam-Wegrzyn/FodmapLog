import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AddMealLogComponent } from './add-meal-log.component.js';

describe('AddMealLogComponent', () => {
  let component: AddMealLogComponent;
  let fixture: ComponentFixture<AddMealLogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [AddMealLogComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(AddMealLogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
