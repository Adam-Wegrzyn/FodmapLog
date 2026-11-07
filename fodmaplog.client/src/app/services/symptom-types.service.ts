import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { SymptomType } from '../domain/SymptomType';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class SymptomTypesService {

   url: string = environment.apiSymptomTypes;
   constructor(private httpClient: HttpClient) { }

  getSymptomTypes(): Observable<SymptomType[]> {
    return this.httpClient.get<SymptomType[]>(`${this.url}`);
  }

  addSymptomType(name: string, scaleKind = 0): Observable<SymptomType> {
    return this.httpClient.post<SymptomType>(`${this.url}/add`, { name, scaleKind });
  }
}
