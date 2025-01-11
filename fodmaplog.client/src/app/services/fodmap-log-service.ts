import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { Product } from "../domain/Product";
import { Injectable } from "@angular/core";
import { MealLog } from "../domain/MealLog";
import { DailyLog } from "../domain/DailyLog";
import { SymptomsLog } from "../domain/SymptomsLog";

@Injectable({
    providedIn: 'root'
})
export class FodmapLogService {
    url: string = "https://localhost:44349/api/FodmapLog"
    constructor(private httpClient: HttpClient) { }

    AddMealLog(mealLog: MealLog): Observable<MealLog> {
        var request = this.httpClient.post<MealLog>(`${this.url}/addMealLog`, mealLog)
        return request;
    }

    GetAllMealLogs(): Observable<MealLog[]> {
        return this.httpClient.get<MealLog[]>(`${this.url}/getAllMealLogs`);
    }

    GetDailyLogsByDate(date: string): Observable<DailyLog[]> {
        return this.httpClient.get<DailyLog[]>(`${this.url}/getDailyLogsByDate/${date}`);
    }
    
    // SymptomsLog methods
    AddSymptomsLog(symptomsLog: SymptomsLog): Observable<SymptomsLog> {
        return this.httpClient.post<SymptomsLog>(`${this.url}/addSymptomsLog`, symptomsLog);
    }

    GetAllSymptomsLogs(): Observable<SymptomsLog[]> {
        return this.httpClient.get<SymptomsLog[]>(`${this.url}/getAllSymptomsLogs`);
    }

    GetDailySymptomsByDate(date: string): Observable<SymptomsLog[]> {
        return this.httpClient.get<SymptomsLog[]>(`${this.url}/getDailySymptomsByDate/${date}`);
    }
}