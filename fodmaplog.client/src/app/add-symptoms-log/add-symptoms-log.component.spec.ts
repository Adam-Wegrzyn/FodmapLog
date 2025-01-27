import { ComponentFixture, TestBed } from '@angular/core/testing';

import { addSymptomsLogComponent } from './add-symptoms-log.component.js';

describe('addSymptomsLogComponent', () => {
  let component: addSymptomsLogComponent;
  let fixture: ComponentFixture<addSymptomsLogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [addSymptomsLogComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(addSymptomsLogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
