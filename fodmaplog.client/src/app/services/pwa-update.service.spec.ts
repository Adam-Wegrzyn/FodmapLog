import { NgZone } from '@angular/core';
import { SwUpdate, UnrecoverableStateEvent, VersionEvent } from '@angular/service-worker';
import { Subject } from 'rxjs';
import { PwaUpdateService } from './pwa-update.service';

function mockUpdates(isEnabled: boolean) {
  const versionUpdates = new Subject<VersionEvent>();
  const unrecoverable = new Subject<UnrecoverableStateEvent>();
  const updates = {
    isEnabled,
    versionUpdates: versionUpdates.asObservable(),
    unrecoverable: unrecoverable.asObservable(),
    checkForUpdate: jasmine.createSpy('checkForUpdate').and.resolveTo(true),
    activateUpdate: jasmine.createSpy('activateUpdate').and.resolveTo(true)
  };
  return { updates, versionUpdates, unrecoverable };
}

describe('PwaUpdateService', () => {
  const zone = new NgZone({ enableLongStackTrace: false });

  it('does not check for updates when the service worker is off', () => {
    const { updates } = mockUpdates(false);
    const service = new PwaUpdateService(zone, updates as unknown as SwUpdate);
    expect(service.available).toBeFalse();
    expect(updates.checkForUpdate).not.toHaveBeenCalled();
  });

  it('shows the banner when a new version is ready', () => {
    const { updates, versionUpdates } = mockUpdates(true);
    const service = new PwaUpdateService(zone, updates as unknown as SwUpdate);
    expect(updates.checkForUpdate).toHaveBeenCalled();

    versionUpdates.next({
      type: 'VERSION_READY',
      currentVersion: { hash: 'old' },
      latestVersion: { hash: 'new' }
    });

    expect(service.available).toBeTrue();
  });

  it('activates the waiting worker and reloads', async () => {
    const { updates } = mockUpdates(true);
    const service = new PwaUpdateService(zone, updates as unknown as SwUpdate);
    const reload = spyOn(service as unknown as { reloadPage: () => void }, 'reloadPage');

    await service.apply();

    expect(updates.activateUpdate).toHaveBeenCalled();
    expect(reload).toHaveBeenCalled();
  });

  it('hides the banner when dismissed', () => {
    const { updates, versionUpdates } = mockUpdates(true);
    const service = new PwaUpdateService(zone, updates as unknown as SwUpdate);
    versionUpdates.next({
      type: 'VERSION_READY',
      currentVersion: { hash: 'old' },
      latestVersion: { hash: 'new' }
    });

    service.dismiss();
    expect(service.available).toBeFalse();
  });
});
