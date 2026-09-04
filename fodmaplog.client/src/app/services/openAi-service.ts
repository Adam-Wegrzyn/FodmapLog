import { Injectable } from "@angular/core";
import { environment } from "../../environments/environment";
import { HttpClient } from "@angular/common/http";
import { catchError, map, Observable, throwError } from "rxjs";
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
        return this.httpClient.post<DailyLog[] | string>(`${this.url}/GenerateMealLogFromAI`, input2)
        .pipe(
            map((data) => this.normalizeDailyLogs(data)),
            catchError((error) => {
                console.error('Error generating meal log from AI:', error);
                return throwError(() => error);
            })
        );
    }

    private normalizeDailyLogs(data: DailyLog[] | string): DailyLog[] {
        let value: unknown = data;
        if (typeof value === 'string') {
            const cleaned = value
                .replace(/^```(?:json)?\s*/i, '')
                .replace(/\s*```$/i, '')
                .trim();
            value = JSON.parse(cleaned);
        }
        if (!Array.isArray(value)) {
            throw new Error('AI response was not a daily-log array');
        }
        return value as DailyLog[];
    }
}

class TranscribeInput {
    transcript: string;
}
