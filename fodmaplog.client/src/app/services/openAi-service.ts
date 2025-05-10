import { Injectable } from "@angular/core";
import { environment } from "../../environments/environment";
import { HttpClient } from "@angular/common/http";
import { catchError, Observable, throwError } from "rxjs";
import { DailyLog } from "../domain/DailyLog";

@Injectable({
    providedIn: 'root'
})
export class OpenAiService {
    private url = environment.apiOpenAi;

    constructor(private httpClient: HttpClient) { }
    
    generateMealLogFromAI(input: string): Observable<DailyLog[]> {
        const input2 = new TranscribeInput();
        input2.transcript = input;
        console.log(input2);
        return this.httpClient.post<DailyLog[]>(`${this.url}/GenerateMealLogFromAI`, input2)
        .pipe(
            catchError((error) => {
                console.error('Error generating meal log from AI:', error);
                return throwError(error);
            })
        );
    }

}
    class TranscribeInput {
        transcript: string;
    }