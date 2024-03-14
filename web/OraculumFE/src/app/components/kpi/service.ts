// global-variable.service.ts
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class GlobalVariableService {
  public sharedVariable: any;

  constructor() {}
}
