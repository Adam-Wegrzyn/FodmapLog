import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { Product } from "../domain/Product";
import { Injectable } from "@angular/core";
import { MealLog } from "../domain/MealLog";

@Injectable({
    providedIn: 'root'
})
export class FodmapLogService{
    url: string = "https://localhost:44349/api/FodmapLog"
    constructor(private httpClient: HttpClient) { }

    AddMealLog(mealLog: MealLog): Observable<MealLog> {
        var request = this.httpClient.post<MealLog>(`${this.url}/addMealLog`, mealLog)
        return request;
    }
}