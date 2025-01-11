import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AddLogBaseComponent } from './add-log-base.component';

describe('AddLogBaseComponent', () => {
  let component: AddLogBaseComponent;
  let fixture: ComponentFixture<AddLogBaseComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [AddLogBaseComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(AddLogBaseComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
