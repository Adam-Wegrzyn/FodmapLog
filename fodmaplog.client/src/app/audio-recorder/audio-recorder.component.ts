import { HttpClient } from '@angular/common/http';
import { Component, EventEmitter, Output } from '@angular/core';
import toWav from 'audiobuffer-to-wav';
import { AudioTranscriptionService } from '../services/audio-transcription.service';
import { AudioBase64 } from '../domain/AudioBase64';

@Component({
  selector: 'app-audio-recorder',
  templateUrl: './audio-recorder.component.html',
  styleUrl: './audio-recorder.component.css'
})
export class AudioRecorderComponent {
  @Output() transcription = new EventEmitter<string>();
  private mediaRecorder: MediaRecorder;
  private audioChunks: Blob[] = [];
  //transcription: string;

  constructor(private http: HttpClient,private audioTrascriptionService: AudioTranscriptionService) {}

  startRecording(): void {
    navigator.mediaDevices.getUserMedia({ audio: true }).then((stream) => {
      this.mediaRecorder = new MediaRecorder(stream);
      this.mediaRecorder.start();

      this.mediaRecorder.ondataavailable = (event) => {
        this.audioChunks.push(event.data);
      };
    });
  }

  stopRecording(): void {
    this.mediaRecorder.stop();
    this.mediaRecorder.onstop = async () => {
      const audioBlob = new Blob(this.audioChunks, { type: 'audio/webm' }); // Default MediaRecorder output
      this.audioChunks = [];
  
      // Convert to ArrayBuffer
      const arrayBuffer = await audioBlob.arrayBuffer();
  
      // Decode audio and convert to WAV
      const audioContext = new AudioContext();
      const audioBuffer = await audioContext.decodeAudioData(arrayBuffer);
      const wavBuffer = toWav(audioBuffer);
  
      // Create a WAV Blob
      const wavBlob = new Blob([wavBuffer], { type: 'audio/wav' });
  
      // Send to Azure
      this.sendAudioToAzure(wavBlob);
  

    };
  }

  sendAudioToAzure(audioBlob: Blob): void {
    const reader = new FileReader();
    reader.readAsArrayBuffer(audioBlob);
    reader.onloadend = () => {
      const converted64 = btoa(
        new Uint8Array(reader.result as ArrayBuffer)
          .reduce((data, byte) => data + String.fromCharCode(byte), '')
      );

    var audioBase64 = {
      value: converted64};

    this.audioTrascriptionService.transcribeAudio(audioBase64).subscribe(
      (response: any) => {console.log('Transcription:', response.transcription);
        this.transcription.emit(response.transcription);
      },
      error => console.error('Error sending audio to Azure:', error)
    );
    // const headers = {
    //   'x-functions-key': ''
    // }; 
    //   this.http
    //     .post('https://example-function-h7ggcqceauheceb9.polandcentral-01.azurewebsites.net/api/Function1/', { audio: base64Audio }, { headers })
    //     .subscribe((response: any) => {
    //       this.transcription.emit(response.transcription);;
    //       console.log('Transcription:', this.transcription);

    //     },
    //     (error) => {
    //       console.error('Error sending audio to Azure:', error);

    //     });  
    // };
    }}}
