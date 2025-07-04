import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { Observable } from 'rxjs';
import { Unit } from '../domain/Unit';

@Injectable({
  providedIn: 'root'
})
export class UnitService {

  url = environment.apiBaseUrl + '/units';
  constructor(private httpClient: HttpClient) {}

    getAllUnits(): Observable<Unit[]> {
        return this.httpClient.get<Unit[]>(`${this.url}`);
   }
}
