import { Injectable, NgZone, Optional } from '@angular/core';
import { SwUpdate, VersionReadyEvent } from '@angular/service-worker';
import { filter } from 'rxjs';

const CHECK_EVERY_MS = 60 * 60 * 1000;

@Injectable({ providedIn: 'root' })
export class PwaUpdateService {
  available = false;
  applying = false;

  constructor(
    private zone: NgZone,
    @Optional() private updates: SwUpdate | null
  ) {
    if (typeof document === 'undefined' || !this.updates?.isEnabled) {
      return;
    }

    this.updates.versionUpdates
      .pipe(filter((event): event is VersionReadyEvent => event.type === 'VERSION_READY'))
      .subscribe(() => this.zone.run(() => {
        this.available = true;
      }));

    this.updates.unrecoverable.subscribe(() => this.zone.run(() => this.reloadPage()));

    void this.check();
    document.addEventListener('visibilitychange', () => {
      if (document.visibilityState === 'visible') {
        void this.check();
      }
    });
    window.setInterval(() => void this.check(), CHECK_EVERY_MS);
  }

  dismiss(): void {
    this.available = false;
  }

  async apply(): Promise<void> {
    if (this.applying) {
      return;
    }
    this.applying = true;
    try {
      if (this.updates?.isEnabled) {
        await this.updates.activateUpdate();
      }
    } finally {
      this.reloadPage();
    }
  }

  private async check(): Promise<void> {
    try {
      await this.updates?.checkForUpdate();
    } catch {
      /* offline or worker not ready */
    }
  }

  private reloadPage(): void {
    window.location.reload();
  }
}
