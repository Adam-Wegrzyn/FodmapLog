import { ComponentFixture, TestBed } from '@angular/core/testing';

import { addMealLogComponent } from './add-meal-log.component.js';

describe('addMealLogComponent', () => {
  let component: addMealLogComponent;
  let fixture: ComponentFixture<addMealLogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [addMealLogComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(addMealLogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
