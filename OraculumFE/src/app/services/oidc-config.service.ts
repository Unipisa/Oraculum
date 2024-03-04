import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class OidcConfigService {
  private configUrl = '/api/oraculum/v1/User/oidc-info';
  constructor(private http: HttpClient) {}

  fetchOidcConfig(): Observable<any> {
    return this.http.get(this.configUrl);
  }
}
