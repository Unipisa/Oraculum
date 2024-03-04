import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root',
})

export class ConfigService {
  contentsForResponse: BehaviorSubject<number> = new BehaviorSubject(5);
  totalContentsHome: BehaviorSubject<number> = new BehaviorSubject(10);
}
