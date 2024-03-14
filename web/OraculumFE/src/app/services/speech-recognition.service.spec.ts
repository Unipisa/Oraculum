/* tslint:disable:no-unused-variable */

import { TestBed, async, inject } from '@angular/core/testing';
import { SpeechRecognitionService } from './speech-recognition.service';

describe('Service: SpeechRecognition2', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [SpeechRecognitionService],
    });
  });

  it('should ...', inject(
    [SpeechRecognitionService],
    (service: SpeechRecognitionService) => {
      expect(service).toBeTruthy();
    }
  ));
});
