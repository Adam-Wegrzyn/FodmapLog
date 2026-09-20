import {
  Component,
  EventEmitter,
  Input,
  NgZone,
  OnChanges,
  OnDestroy,
  Output,
  SimpleChanges
} from '@angular/core';
import toWav from 'audiobuffer-to-wav';
import { AudioTranscriptionService } from '../services/audio-transcription.service';
import { LanguageService } from '../services/language.service';
import { TranslateService } from '@ngx-translate/core';
import { faMicrophone, faStop } from '@fortawesome/free-solid-svg-icons';

export type RecorderUiState = 'idle' | 'recording' | 'transcribing' | 'error';

@Component({
  selector: 'app-audio-recorder',
  templateUrl: './audio-recorder.component.html',
  styleUrl: './audio-recorder.component.css'
})
export class AudioRecorderComponent implements OnDestroy, OnChanges {
  /** When false, hide the mic FAB (e.g. while the review sheet is open). */
  @Input() interactive = true;

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

  constructor(
    private audioTrascriptionService: AudioTranscriptionService,
    private ngZone: NgZone,
    private language: LanguageService,
    private translate: TranslateService
  ) {}

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['interactive'] && !this.interactive && this.state === 'recording') {
      this.cancelRecording();
    }
  }

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
    if (!this.interactive) {
      return;
    }
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
      this.setError(this.translate.instant('recorder.unsupported'));
      return;
    }

    navigator.mediaDevices.getUserMedia({
      audio: {
        channelCount: 1,
        echoCancellation: true,
        noiseSuppression: true
      }
    }).then((stream) => {
      this.ngZone.run(() => {
        this.mediaStream = stream;
        this.audioChunks = [];
        const mime = MediaRecorder.isTypeSupported('audio/webm;codecs=opus')
          ? 'audio/webm;codecs=opus'
          : undefined;
        this.mediaRecorder = mime
          ? new MediaRecorder(stream, { mimeType: mime, audioBitsPerSecond: 24000 })
          : new MediaRecorder(stream);

        this.mediaRecorder.ondataavailable = (event) => {
          if (event.data?.size > 0) {
            this.audioChunks.push(event.data);
          }
        };
        // Timeslice keeps data flowing; does not auto-stop on silence.
        this.mediaRecorder.start(1000);
        this.elapsedSeconds = 0;
        this.setState('recording');
        this.clearTimer();
        this.timerId = setInterval(() => {
          this.ngZone.run(() => {
            this.elapsedSeconds += 1;
          });
        }, 1000);
      });
    }).catch((err: DOMException | Error) => {
      this.ngZone.run(() => {
        const name = (err as DOMException).name || '';
        if (name === 'NotAllowedError' || name === 'PermissionDeniedError') {
          this.setError(this.translate.instant('recorder.denied'));
        } else if (name === 'NotFoundError') {
          this.setError(this.translate.instant('recorder.notFound'));
        } else {
          this.setError(this.translate.instant('recorder.startFail'));
        }
      });
    });
  }

  stopRecording(): void {
    if (!this.mediaRecorder || this.state !== 'recording') {
      return;
    }

    this.clearTimer();
    this.setState('transcribing');

    this.mediaRecorder.onstop = () => {
      void this.ngZone.run(async () => {
        try {
          const audioBlob = new Blob(this.audioChunks, {
            type: this.mediaRecorder?.mimeType || 'audio/webm'
          });
          this.audioChunks = [];
          this.stopMediaTracks();

          // 16 kHz mono WAV — much smaller upload; matches Azure Speech defaults.
          const wavBlob = await this.toSpeechWav(audioBlob);
          this.sendAudioToAzure(wavBlob);
        } catch {
          this.setError(this.translate.instant('recorder.readFail'));
          this.setState('idle');
        }
      });
    };

    try {
      if (this.mediaRecorder.state === 'recording') {
        this.mediaRecorder.requestData();
      }
    } catch { /* ignore */ }
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

  private async toSpeechWav(blob: Blob): Promise<Blob> {
    const arrayBuffer = await blob.arrayBuffer();
    const decodeCtx = new AudioContext();
    let decoded: AudioBuffer;
    try {
      decoded = await decodeCtx.decodeAudioData(arrayBuffer.slice(0));
    } finally {
      await decodeCtx.close();
    }

    const targetRate = 16000;
    const frameCount = Math.max(1, Math.ceil(decoded.duration * targetRate));
    const offline = new OfflineAudioContext(1, frameCount, targetRate);

    const mono = offline.createBuffer(1, decoded.length, decoded.sampleRate);
    const out = mono.getChannelData(0);
    const channels = decoded.numberOfChannels;
    for (let i = 0; i < decoded.length; i++) {
      let sum = 0;
      for (let c = 0; c < channels; c++) {
        sum += decoded.getChannelData(c)[i];
      }
      out[i] = sum / channels;
    }

    const source = offline.createBufferSource();
    source.buffer = mono;
    source.connect(offline.destination);
    source.start(0);
    const rendered = await offline.startRendering();
    const wavBuffer = toWav(rendered);
    return new Blob([wavBuffer], { type: 'audio/wav' });
  }

  private sendAudioToAzure(audioBlob: Blob): void {
    const reader = new FileReader();
    reader.onloadend = () => {
      this.ngZone.run(() => {
        try {
          const buffer = reader.result as ArrayBuffer;
          const bytes = new Uint8Array(buffer);
          let binary = '';
          const chunk = 0x8000;
          for (let i = 0; i < bytes.length; i += chunk) {
            binary += String.fromCharCode(...bytes.subarray(i, i + chunk));
          }
          const converted64 = btoa(binary);

          this.audioTrascriptionService.transcribeAudio({
            value: converted64,
            language: this.language.speechLocale
          }).subscribe({
            next: (response: { transcription?: string }) => {
              const text = (response?.transcription || '').trim();
              if (!text) {
                this.setError(this.translate.instant('recorder.noSpeech'));
                this.setState('idle');
                return;
              }
              this.setState('idle');
              this.transcription.emit(text);
            },
            error: (err) => {
              const status = err?.status;
              if (status === 429) {
                this.setError(this.translate.instant('daily.aiRateLimited'));
              } else if (status === 503) {
                this.setError(this.translate.instant('recorder.unavailable'));
              } else {
                this.setError(this.translate.instant('recorder.transcribeFail'));
              }
              this.setState('idle');
            }
          });
        } catch {
          this.setError(this.translate.instant('recorder.readFail'));
          this.setState('idle');
        }
      });
    };
    reader.onerror = () => {
      this.ngZone.run(() => {
        this.setError(this.translate.instant('recorder.readFail'));
        this.setState('idle');
      });
    };
    reader.readAsArrayBuffer(audioBlob);
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
