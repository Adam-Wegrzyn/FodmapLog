import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AudioTranscriptionService {
  url =  environment.apiAudioTranscription;
  
  constructor(private httpClient: HttpClient) { }

  transcribeAudio(audioBase64: string): Observable<string>{
    return this.httpClient.post<string>(this.url, { audio: audioBase64 });
  }
}
