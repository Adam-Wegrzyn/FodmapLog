import { TestBed } from '@angular/core/testing';

import { SymptomTypesService } from './symptom-types.service';

describe('SymptomTypesService', () => {
  let service: SymptomTypesService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(SymptomTypesService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
