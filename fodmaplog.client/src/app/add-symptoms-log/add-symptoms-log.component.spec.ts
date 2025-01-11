import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AddSymptomsLogComponent } from './add-symptoms-log.component.js';

describe('AddSymptomsLogComponent', () => {
  let component: AddSymptomsLogComponent;
  let fixture: ComponentFixture<AddSymptomsLogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [AddSymptomsLogComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(AddSymptomsLogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
