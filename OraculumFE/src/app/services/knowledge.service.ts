import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Fact } from '../interfaces/fact';
import { SibyllaConfig } from '../interfaces/sibyllaConfig';

interface QueryPayload {
  query: string;
  distance: number;
  limit?: number;
  autoCut?: number;
  factTypeFilter?: (string | null | undefined)[] | null;
  categoryFilter?: string[];
  tagsFilter?: string[];
}

export interface QueryPayload_for_facts {
  messageId: string,
  factId: string,
  questionMessageId: string,
  rating: string,
}

@Injectable({
  providedIn: 'root',
})
export class KnowledgeService {
  private apiUrl = '/api/oraculum/v1/BackOffice/facts';
  private apiUrl_for_feedback = '/api/oraculum/v1/FrontOffice/sibylla' 

  constructor(private http: HttpClient) {}

  postFeedbackFact(payload: QueryPayload_for_facts, sibyllaId: string,chatId: string): Observable<any> {
    return this.http.post(this.apiUrl_for_feedback + '/'+sibyllaId+'/chat/'+chatId+'/feedback', payload);
  }

  postQuery(payload: QueryPayload): Observable<any> {
    return this.http.post(this.apiUrl + '/query', payload);
  }

  getFacts(
    limit: number,
    offset: number,
    sort: string,
    order: string
  ): Observable<any> {
    const params = { limit, offset, sort, order };
    return this.http.get(this.apiUrl, { params });
  }

  postFact(fact: Fact): Observable<any> {
    return this.http.post(this.apiUrl, [fact]);
  }

  updateFact(fact: any): Observable<any> {
    return this.http.put(this.apiUrl, fact);
  }

  deleteFact(id: string): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }

  getFactById(id: string): Observable<any> {
    return this.http.get(`${this.apiUrl}/${id}`);
  }

  getSibyllaeConfigs(): Observable<Array<SibyllaConfig>> {
    return this.http.get<Array<SibyllaConfig>>('/api/oraculum/v1/BackOffice/sibylla-configs');
  }
}
