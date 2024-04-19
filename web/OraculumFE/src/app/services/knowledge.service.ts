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
  messageId: string;
  factId: string;
  questionMessageId: string;
  rating: string;
  text?: string;
}

export interface taskInfo {
  taskId: string;
  status?: any;
}

@Injectable({
  providedIn: 'root',
})
export class KnowledgeService {
  private apiUrl = '/api/oraculum/v1/BackOffice/facts';
  private apiUrl_for_feedback = '/api/oraculum/v1/FrontOffice/sibylla';
  private apiUrl_ingest_text = '/api/oraculum/v1/DataIngestion/factsFromText';
  private apiUrl_ingest_webpage =
    '/api/oraculum/v1/DataIngestion/factsFromWebPages';
  private apiUrl_ingest_document =
    '/api/oraculum/v1/DataIngestion/factsFromDocuments';
  private apiUrl_ingest_audioVideo =
    '/api/oraculum/v1/DataIngestion/factsFromAudioVideo';

  constructor(private http: HttpClient) {}

  postFeedbackFact(
    payload: QueryPayload_for_facts,
    sibyllaId: string,
    chatId: string
  ): Observable<any> {
    return this.http.post(
      this.apiUrl_for_feedback +
        '/' +
        sibyllaId +
        '/chat/' +
        chatId +
        '/feedback',
      payload
    );
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
    return this.http.get<Array<SibyllaConfig>>(
      '/api/oraculum/v1/BackOffice/sibylla-configs'
    );
  }

  ingestText(text: string, category: string): Observable<any> {
    const params = { category };
    const payload = [{
      content: text,
    }];
    return this.http.post(this.apiUrl_ingest_text, payload, { params });
}


ingestWebpage(url: string[], category: string): Observable<any> {
  const params = { category };
  const payload = {
    origin: [url],  
  };
  return this.http.post(this.apiUrl_ingest_webpage, payload, { params });
}

  ingestDocument(file: File, category: string): Observable<any> {
    const params = { category };
    const formData: FormData = new FormData();
    formData.append('file', file, file.name);
    return this.http.post(this.apiUrl_ingest_document, formData, { params });
  }

  ingestAudioVideo(file: File, category: string): Observable<any> {
    const formData: FormData = new FormData();
    formData.append('file', file); 
    const params = { category };
    return this.http.post(this.apiUrl_ingest_audioVideo, formData, { params });
}

  checkTaskStatus(taskId: string): Observable<any> {
    return this.http.get(
      `/api/oraculum/v1/DataIngestion/checkStatus/${taskId}`
    );
  }
}
