import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface InsightsProgress {
  daysWithEntries: number;
  daysRequired: number;
  meals: number;
  mealsRequired: number;
  symptomLogs: number;
  symptomLogsRequired: number;
}

export interface InsightFinding {
  product: string;
  foodKey: string;
  exposures: number;
  hits: number;
  baselineHits: number;
  baselineMeals: number;
  label: string;
  symptomNames: string[];
  eatenWith: string[];
}

export interface InsightRareFood {
  product: string;
  foodKey: string;
  exposures: number;
  hits: number;
  baselineHits: number;
  baselineMeals: number;
}

export interface InsightsResponse {
  status: 'locked' | 'ready' | string;
  days: number;
  progress: InsightsProgress;
  findings: InsightFinding[];
  rarelyFollowed: InsightRareFood[];
}

export interface InsightFeedbackRequest {
  foodKey: string;
  product: string;
  helpful: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class InsightsService {
  private readonly url = `${environment.apiBaseUrl}/Insights`;

  constructor(private http: HttpClient) {}

  getInsights(days = 14): Observable<InsightsResponse> {
    return this.http.get<InsightsResponse>(`${this.url}/getInsights`, {
      params: { days }
    });
  }

  sendFeedback(body: InsightFeedbackRequest): Observable<{ saved: boolean }> {
    return this.http.post<{ saved: boolean }>(`${this.url}/feedback`, body);
  }
}
