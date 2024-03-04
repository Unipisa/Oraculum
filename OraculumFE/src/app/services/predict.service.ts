import {
  HttpClient,
  HttpErrorResponse,
  HttpHeaders,
} from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, catchError, throwError } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class PredictService {
  constructor(private http: HttpClient) {}

  askExplain(
    question: string,
    limit: number,
    oneshot?: boolean
  ): Observable<any> {
    const body = {
      Query: question,
      Limit: limit,
      OneShot: oneshot || false,
    };
    const headers = new HttpHeaders({
      'Content-Type': 'application/json',
    });

    return this.http
      .post('/api/oraculum/v1/FrontOffice/debug/answer', body, {
        headers: headers,
      })
      .pipe(catchError(this.handleError.bind(this)));
  }

  askExplainNew(
    text: string,
    sibyllaId: string,
    chatId: string
  ): Observable<any> {
    const body = {
      text: text,
    };
    const apiUrl = `/api/oraculum/v1/FrontOffice/sibylla/${sibyllaId}/chat-explain/${chatId}/message`;
    const headers = new HttpHeaders({
      'Content-Type': 'application/json',
    });

    return this.http
      .post(apiUrl, body, {
        headers: headers,
      })
      .pipe(catchError(this.handleError.bind(this)));
  }

  askPrediction(question: string): Observable<any> {
    let body = {
      text: question,
    };
    return this.http
      .post('api/predict', body)
      .pipe(catchError(this.handleError.bind(this)));
  }

  askAsync(question: string): Observable<any> {
    return this.http
      .post(`/api/oraculum/v1/FrontOffice/answer/${question}`, null, {
        responseType: 'text',
      })
      .pipe(catchError(this.handleError.bind(this)));
  }

  getAnswerById(id: string): Observable<any> {
    return this.http
      .get(`/api/oraculum/v1/FrontOffice/getanswer/${id}`, { responseType: 'text' })
      .pipe(catchError(this.handleError.bind(this)));
  }

  private handleError(error: HttpErrorResponse) {
    if (error.error instanceof ErrorEvent) {
      console.error('An error occured:', error.error.message);
    } else {
      console.error(
        `Backend returned code "${error.status}", body was "${error.statusText}"`
      );
    }

    return throwError(error);
  }
}
