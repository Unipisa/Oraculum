/* tslint:disable:no-unused-variable */

import { TestBed, async, inject } from '@angular/core/testing';
import { PredictService } from './predict.service';

describe('Service: Predict', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [PredictService]
    });
  });

  it('should ...', inject([PredictService], (service: PredictService) => {
    expect(service).toBeTruthy();
  }));
});
