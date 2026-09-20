import { Injectable } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { BehaviorSubject, firstValueFrom } from 'rxjs';

export type AppLang = 'en' | 'pl';

const STORAGE_KEY = 'hg-lang';

@Injectable({ providedIn: 'root' })
export class LanguageService {
  private readonly langSubject = new BehaviorSubject<AppLang>('en');
  readonly lang$ = this.langSubject.asObservable();

  constructor(private translate: TranslateService) {}

  /** Resolve browser/saved language and load translations. */
  init(): Promise<unknown> {
    this.translate.addLangs(['en', 'pl']);
    this.translate.setDefaultLang('en');
    const lang = this.resolveInitial();
    return this.use(lang, false);
  }

  get currentLang(): AppLang {
    return this.langSubject.value;
  }

  /** BCP-47 locale for Angular date pipe. */
  get dateLocale(): string {
    return this.currentLang === 'pl' ? 'pl' : 'en-US';
  }

  /** Azure Speech recognition locale. */
  get speechLocale(): string {
    return this.currentLang === 'pl' ? 'pl-PL' : 'en-US';
  }

  use(lang: AppLang, persist = true): Promise<unknown> {
    if (persist) {
      localStorage.setItem(STORAGE_KEY, lang);
    }
    this.langSubject.next(lang);
    document.documentElement.lang = lang === 'pl' ? 'pl' : 'en';
    return firstValueFrom(this.translate.use(lang));
  }

  setLang(lang: AppLang): void {
    void this.use(lang, true);
  }

  private resolveInitial(): AppLang {
    const saved = localStorage.getItem(STORAGE_KEY);
    if (saved === 'en' || saved === 'pl') {
      return saved;
    }
    const nav = (navigator.language || '').toLowerCase();
    return nav.startsWith('pl') ? 'pl' : 'en';
  }
}
