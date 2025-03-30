import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { Product } from "../domain/Product";
import { Injectable } from "@angular/core";
import { MealLog } from "../domain/MealLog";
import { DailyLog } from "../domain/DailyLog";
import { SymptomsLog } from "../domain/SymptomsLog";
import { environment } from "../../environments/environment";

@Injectable({
    providedIn: 'root'
})
export class FodmapLogService {
    url: string = environment.apiUrl
    constructor(private httpClient: HttpClient) { }

    updateMealLog(mealLog: MealLog): Observable<MealLog> {
        return this.httpClient.put<MealLog>(`${this.url}/updateMealLog`, mealLog);
      }
    addMealLog(mealLog: MealLog): Observable<MealLog> {
        return this.httpClient.post<MealLog>(`${this.url}/addMealLog`, mealLog);
    }

    getAllMealLogs(): Observable<MealLog[]> {
        return this.httpClient.get<MealLog[]>(`${this.url}/getAllMealLogs`);
    }

    getMealLogById(id: number): Observable<MealLog> {
        return this.httpClient.get<MealLog>(`${this.url}/getMealLogById/${id}`);
    }

    getDailyLogsByDate(date: string): Observable<DailyLog[]> {
        return this.httpClient.get<DailyLog[]>(`${this.url}/getDailyLogsByDate/${date}`);
    }
    
    // SymptomsLog methods
    addSymptomsLog(symptomsLog: SymptomsLog): Observable<SymptomsLog> {
        return this.httpClient.post<SymptomsLog>(`${this.url}/addSymptomsLog`, symptomsLog);
    }

    getAllSymptomsLogs(): Observable<SymptomsLog[]> {
        return this.httpClient.get<SymptomsLog[]>(`${this.url}/getAllSymptomsLogs`);
    }

    getDailySymptomsByDate(date: string): Observable<SymptomsLog[]> {
        return this.httpClient.get<SymptomsLog[]>(`${this.url}/getDailySymptomsByDate/${date}`);
    }
    getSymptomsLogById(id: string): Observable<SymptomsLog> {
        return this.httpClient.get<SymptomsLog>(`${this.url}/getSymptomsLogById/${id}`);
      }
    updateSymptomsLog(symptomsLog: SymptomsLog): Observable<SymptomsLog> {
        return this.httpClient.put<SymptomsLog>(`${this.url}/updateSymptomsLog`, symptomsLog);
      }
    
}