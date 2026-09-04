import { Component, EventEmitter, OnDestroy, Output } from '@angular/core';
import toWav from 'audiobuffer-to-wav';
import { AudioTranscriptionService } from '../services/audio-transcription.service';
import { faMicrophone, faStop } from '@fortawesome/free-solid-svg-icons';

export type RecorderUiState = 'idle' | 'recording' | 'transcribing' | 'error';

@Component({
  selector: 'app-audio-recorder',
  templateUrl: './audio-recorder.component.html',
  styleUrl: './audio-recorder.component.css'
})
export class AudioRecorderComponent implements OnDestroy {
  @Output() transcription = new EventEmitter<string>();
  @Output() stateChange = new EventEmitter<RecorderUiState>();
  @Output() errorChange = new EventEmitter<string | null>();

  faMicrophone = faMicrophone;
  faStop = faStop;

  state: RecorderUiState = 'idle';
  errorMessage: string | null = null;
  elapsedSeconds = 0;

  private mediaRecorder: MediaRecorder | null = null;
  private mediaStream: MediaStream | null = null;
  private audioChunks: Blob[] = [];
  private timerId: ReturnType<typeof setInterval> | null = null;

  constructor(private audioTrascriptionService: AudioTranscriptionService) {}

  ngOnDestroy(): void {
    this.clearTimer();
    this.stopMediaTracks();
  }

  get isBusy(): boolean {
    return this.state === 'recording' || this.state === 'transcribing';
  }

  get elapsedLabel(): string {
    const m = Math.floor(this.elapsedSeconds / 60).toString().padStart(2, '0');
    const s = (this.elapsedSeconds % 60).toString().padStart(2, '0');
    return `${m}:${s}`;
  }

  onMicClick(): void {
    if (this.state === 'recording') {
      this.stopRecording();
      return;
    }
    if (this.state === 'transcribing') {
      return;
    }
    this.startRecording();
  }

  /** Public so parent can restart after review "Re-record". */
  startRecording(): void {
    this.clearError();
    if (!navigator.mediaDevices?.getUserMedia) {
      this.setError('Microphone is not supported in this browser.');
      return;
    }

    navigator.mediaDevices.getUserMedia({ audio: true }).then((stream) => {
      this.mediaStream = stream;
      this.audioChunks = [];
      this.mediaRecorder = new MediaRecorder(stream);
      this.mediaRecorder.ondataavailable = (event) => {
        if (event.data?.size > 0) {
          this.audioChunks.push(event.data);
        }
      };
      this.mediaRecorder.start();
      this.elapsedSeconds = 0;
      this.setState('recording');
      this.clearTimer();
      this.timerId = setInterval(() => {
        this.elapsedSeconds += 1;
      }, 1000);
    }).catch((err: DOMException | Error) => {
      const name = (err as DOMException).name || '';
      if (name === 'NotAllowedError' || name === 'PermissionDeniedError') {
        this.setError('Microphone permission denied. Allow mic access and try again.');
      } else if (name === 'NotFoundError') {
        this.setError('No microphone found. Connect a mic and try again.');
      } else {
        this.setError('Could not start recording. Check microphone settings.');
      }
    });
  }

  stopRecording(): void {
    if (!this.mediaRecorder || this.state !== 'recording') {
      return;
    }

    this.clearTimer();
    this.setState('transcribing');

    this.mediaRecorder.onstop = async () => {
      try {
        const audioBlob = new Blob(this.audioChunks, { type: 'audio/webm' });
        this.audioChunks = [];
        this.stopMediaTracks();

        const arrayBuffer = await audioBlob.arrayBuffer();
        const audioContext = new AudioContext();
        const audioBuffer = await audioContext.decodeAudioData(arrayBuffer);
        const wavBuffer = toWav(audioBuffer);
        const wavBlob = new Blob([wavBuffer], { type: 'audio/wav' });
        await audioContext.close();

        this.sendAudioToAzure(wavBlob);
      } catch {
        this.setError('Could not process the recording. Please try again.');
        this.setState('idle');
      }
    };

    this.mediaRecorder.stop();
  }

  cancelRecording(): void {
    this.clearTimer();
    if (this.mediaRecorder && this.state === 'recording') {
      this.mediaRecorder.onstop = null;
      try {
        this.mediaRecorder.stop();
      } catch { /* ignore */ }
    }
    this.audioChunks = [];
    this.stopMediaTracks();
    this.setState('idle');
  }

  private sendAudioToAzure(audioBlob: Blob): void {
    const reader = new FileReader();
    reader.readAsArrayBuffer(audioBlob);
    reader.onloadend = () => {
      const converted64 = btoa(
        new Uint8Array(reader.result as ArrayBuffer)
          .reduce((data, byte) => data + String.fromCharCode(byte), '')
      );

      this.audioTrascriptionService.transcribeAudio({ value: converted64 }).subscribe({
        next: (response: { transcription?: string }) => {
          const text = (response?.transcription || '').trim();
          if (!text) {
            this.setError('No speech detected. Tap the mic and try again.');
            this.setState('idle');
            return;
          }
          this.setState('idle');
          this.transcription.emit(text);
        },
        error: () => {
          this.setError('Transcription failed. Check your connection and try again.');
          this.setState('idle');
        }
      });
    };
    reader.onerror = () => {
      this.setError('Could not read the recording. Please try again.');
      this.setState('idle');
    };
  }

  private setState(state: RecorderUiState): void {
    this.state = state;
    this.stateChange.emit(state);
  }

  private setError(message: string): void {
    this.errorMessage = message;
    this.state = 'error';
    this.errorChange.emit(message);
    this.stateChange.emit('error');
  }

  private clearError(): void {
    this.errorMessage = null;
    this.errorChange.emit(null);
  }

  private clearTimer(): void {
    if (this.timerId) {
      clearInterval(this.timerId);
      this.timerId = null;
    }
  }

  private stopMediaTracks(): void {
    this.mediaStream?.getTracks().forEach(t => t.stop());
    this.mediaStream = null;
    this.mediaRecorder = null;
  }
}
