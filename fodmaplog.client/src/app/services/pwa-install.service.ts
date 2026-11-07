import { Injectable, NgZone } from '@angular/core';

const DISMISSED_AT_KEY = 'mova.pwaInstall.dismissedAt';
const DONE_KEY = 'mova.pwaInstall.done';
const SNOOZE_MS = 14 * 24 * 60 * 60 * 1000;

interface BeforeInstallPromptEvent extends Event {
  prompt(): Promise<void>;
  userChoice: Promise<{ outcome: 'accepted' | 'dismissed' }>;
}

@Injectable({ providedIn: 'root' })
export class PwaInstallService {
  visible = false;
  mode: 'chrome' | 'ios' | null = null;
  private deferred: BeforeInstallPromptEvent | null = null;

  constructor(private zone: NgZone) {
    if (typeof window === 'undefined') {
      return;
    }

    window.addEventListener('beforeinstallprompt', (event: Event) => {
      event.preventDefault();
      this.zone.run(() => {
        this.deferred = event as BeforeInstallPromptEvent;
        this.mode = 'chrome';
        this.refresh();
      });
    });

    window.addEventListener('appinstalled', () => {
      this.zone.run(() => this.markDone());
    });

    if (this.isIosSafari() && !this.isStandalone()) {
      this.mode = 'ios';
    }
  }

  refresh(): void {
    if (this.shouldShow()) {
      this.visible = true;
    }
  }

  dismiss(): void {
    this.visible = false;
    try {
      localStorage.setItem(DISMISSED_AT_KEY, String(Date.now()));
    } catch {
      /* private mode */
    }
  }

  async install(): Promise<void> {
    if (this.mode === 'ios') {
      this.dismiss();
      return;
    }

    if (!this.deferred) {
      this.dismiss();
      return;
    }

    const promptEvent = this.deferred;
    this.deferred = null;
    await promptEvent.prompt();
    const choice = await promptEvent.userChoice;
    if (choice.outcome === 'accepted') {
      this.markDone();
    } else {
      this.dismiss();
    }
  }

  private shouldShow(): boolean {
    if (this.isStandalone() || this.isDone() || this.isSnoozed()) {
      return false;
    }
    return this.mode === 'chrome' || this.mode === 'ios';
  }

  private markDone(): void {
    this.visible = false;
    this.deferred = null;
    this.mode = null;
    try {
      localStorage.setItem(DONE_KEY, '1');
    } catch {
      /* private mode */
    }
  }

  private isStandalone(): boolean {
    const nav = window.navigator as Navigator & { standalone?: boolean };
    return window.matchMedia('(display-mode: standalone)').matches || nav.standalone === true;
  }

  private isIosSafari(): boolean {
    const ua = window.navigator.userAgent;
    const iOS = /iPad|iPhone|iPod/.test(ua) || (navigator.platform === 'MacIntel' && navigator.maxTouchPoints > 1);
    const webkit = /WebKit/.test(ua);
    const notOther = !/CriOS|FxiOS|EdgiOS|OPiOS|Chrome|Android/.test(ua);
    return iOS && webkit && notOther;
  }

  private isDone(): boolean {
    try {
      return localStorage.getItem(DONE_KEY) === '1';
    } catch {
      return false;
    }
  }

  private isSnoozed(): boolean {
    try {
      const raw = localStorage.getItem(DISMISSED_AT_KEY);
      if (!raw) {
        return false;
      }
      const at = Number(raw);
      return Number.isFinite(at) && Date.now() - at < SNOOZE_MS;
    } catch {
      return false;
    }
  }
}
