import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { AudioBase64 } from '../domain/AudioBase64';

@Injectable({
  providedIn: 'root'
})
export class AudioTranscriptionService {
  url =  environment.apiAudioTranscription;
  
  constructor(private httpClient: HttpClient) { }

  transcribeAudio(audioBase64: AudioBase64): Observable<{ transcription: string }> {
    return this.httpClient.post<{ transcription: string }>(`${this.url}/transcribe`, audioBase64);
  }
}
