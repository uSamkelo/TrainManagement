import { TestBed } from '@angular/core/testing';

import { TrainPositionService } from './train-position.service';

describe('TrainPositionService', () => {
  let service: TrainPositionService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(TrainPositionService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
