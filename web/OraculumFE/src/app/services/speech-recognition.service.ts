// speech-recognition.service.ts
import { Injectable, NgZone } from '@angular/core';
import { Observable, Subject } from 'rxjs';

interface IWindow extends Window {
  webkitSpeechRecognition: any;
}

@Injectable({
  providedIn: 'root',
})
export class SpeechRecognitionService {
  speechRecognition: any;

  constructor(private zone: NgZone) {}

  record(): Observable<string> {
    return new Observable((observer) => {
      const { webkitSpeechRecognition }: IWindow = window as unknown as IWindow;
      this.speechRecognition = new webkitSpeechRecognition();
      this.speechRecognition.continuous = false;
      this.speechRecognition.lang = 'it-IT';
      this.speechRecognition.maxAlternatives = 1;
      this.speechRecognition.interimResults = true;
      this.speechRecognition.onresult = (speech: { results: any }) => {
        let term: string = '';
        for (const result of speech.results) {
          term += result[0].transcript;
        }
        this.zone.run(() => {
          observer.next(term);
        });
      };
      this.speechRecognition.onerror = (error: any) => {
        observer.error(error);
      };
      this.speechRecognition.onend = () => {
        console.log('Speech recognition service is done');
        this.stop();
        observer.complete();
      };
      this.speechRecognition.start();
      console.log('Say something - I am listening...');
    });
  }

  stop() {
    if (this.speechRecognition) {
      this.speechRecognition.stop();
    }
  }
}
