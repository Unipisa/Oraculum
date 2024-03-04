// stream.service.ts
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class StreamService {
  private worker: Worker;

  constructor() {
    this.worker = new Worker('assets/stream.worker.js', { type: 'module' });
  }

  private apiUrl = '/api/oraculum/v1/FrontOffice/sibylla/';
  getStreamData2(sibyllaId: string, chatId: string, text: string): Observable<any> {
    return new Observable(observer => {
      const fullUrl = `${this.apiUrl}${sibyllaId}/chat/${chatId}/message`;
      const requestBody = { text };

      fetch(fullUrl, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(requestBody)
      })
      .then(response => {
        if (!response.body) {
          throw new Error('ReadableStream not supported in this browser.');
        }
        const reader = response.body.getReader();
        let accumulatedText = '';

        function read() {
          reader.read().then(({ done, value }) => {
            if (done) {
              observer.complete();
              return;
            }

            const chunk = new TextDecoder().decode(value, { stream: true });
            accumulatedText += chunk;

            try {
              while (accumulatedText.includes('}\n{')) { // Assuming each JSON object is separated by a newline
                const endIndex = accumulatedText.indexOf('}\n{') + 1;
                const jsonText = accumulatedText.substring(0, endIndex);
                accumulatedText = accumulatedText.substring(endIndex);

                const json = JSON.parse(jsonText);
                observer.next(json);
              }
            } catch (error) {
              console.error('Error parsing JSON:', error);
            }

            read();
          }).catch(error => {
            console.error('Stream reading error:', error);
            observer.error(error);
          });
        }

        read();
      })
      .catch(error => {
        console.error('Fetch error:', error);
        observer.error(error);
      });
    });
  }

  getStreamDataNew(
text: string, chatId: string, sibyllaId: string, accessToken?: string
  ): Observable<any> {
    this.worker = new Worker('assets/stream.worker.js', { type: 'module' });
    const apiUrl = `/api/oraculum/v1/FrontOffice/sibylla/${sibyllaId}/chat/${chatId}/message`;
    const requestData = {"text": text};

    return new Observable((observer) => {
      this.worker.postMessage({ apiUrl, requestData, accessToken });

      this.worker.onmessage = ({ data }) => {
        if (data === 'END_OF_STREAM') {
          observer.complete();
        } else {
          observer.next(data);
        }
      };

      this.worker.onerror = (error) => {
        observer.error(error);
      };

      return () => {
        this.worker.terminate();
      };
    });
  }

  getStreamData(
    question: string,
    requestData?: any,
    clearSession?: boolean
  ): Observable<string> {
    this.worker = new Worker('assets/stream.worker.js', { type: 'module' });
    // Append clearSession as a query parameter if it is provided and true
    const clearSessionParam = clearSession
      ? `?clearSession=${clearSession}`
      : `?clearSession=${false}`;
    const apiUrl = `/api/oraculum/v1/FrontOffice/answerStream/${question}${clearSessionParam}`;
    requestData = {};

    return new Observable((observer) => {
      this.worker.postMessage({ apiUrl, requestData });

      this.worker.onmessage = ({ data }) => {
        if (data === 'END_OF_STREAM') {
          observer.complete();
        } else {
          observer.next(data);
        }
      };

      this.worker.onerror = (error) => {
        observer.error(error);
      };

      return () => {
        this.worker.terminate();
      };
    });
  }
}
