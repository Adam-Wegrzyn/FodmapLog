import {
  Component,
  Input,
  OnChanges,
  OnDestroy,
  OnInit,
  SimpleChanges
} from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { Subscription } from 'rxjs';
import { DailyLog } from '../domain/DailyLog';
import { Unit } from '../domain/Unit';
import {
  translateScale,
  translateSymptomType,
  translateUnit
} from '../services/reference-i18n';

export interface TimelineMarker {
  key: string;
  kind: 'meal' | 'symptom';
  at: Date;
  /** 0–100 position on the day axis (midnight → midnight). */
  percent: number;
  timeLabel: string;
  summary: string;
  lines: string[];
  severity?: number;
  tone?: 'calm' | 'mild' | 'hot';
}

/** Day axis starts at midnight; 24 hours across the track. */
const DAY_MINUTES = 24 * 60;

@Component({
  selector: 'app-day-timeline',
  templateUrl: './day-timeline.component.html',
  styleUrl: './day-timeline.component.css'
})
export class DayTimelineComponent implements OnInit, OnChanges, OnDestroy {
  @Input() logs: DailyLog[] = [];
  @Input() dateLocale = 'en-US';

  mealMarkers: TimelineMarker[] = [];
  symptomMarkers: TimelineMarker[] = [];
  selected: TimelineMarker | null = null;
  /** Soft “likely reaction” band after a selected meal (2–6h). */
  windowStartPct: number | null = null;
  windowEndPct: number | null = null;

  readonly hourTicks = [0, 6, 12, 18, 24];
  private langSub?: Subscription;

  constructor(private translate: TranslateService) {}

  ngOnInit(): void {
    this.langSub = this.translate.onLangChange.subscribe(() => this.rebuild());
  }

  ngOnDestroy(): void {
    this.langSub?.unsubscribe();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['logs'] || changes['dateLocale']) {
      this.rebuild();
    }
  }

  get hasEvents(): boolean {
    return this.mealMarkers.length > 0 || this.symptomMarkers.length > 0;
  }

  select(marker: TimelineMarker, event?: Event): void {
    event?.stopPropagation();
    if (this.selected?.key === marker.key) {
      this.clearSelection();
      return;
    }
    this.selected = marker;
    if (marker.kind === 'meal') {
      this.windowStartPct = this.clampPct(marker.percent + (2 / 24) * 100);
      this.windowEndPct = this.clampPct(marker.percent + (6 / 24) * 100);
    } else {
      this.windowStartPct = null;
      this.windowEndPct = null;
    }
  }

  clearSelection(): void {
    this.selected = null;
    this.windowStartPct = null;
    this.windowEndPct = null;
  }

  trackByKey(_: number, m: TimelineMarker): string {
    return m.key;
  }

  private rebuild(): void {
    const meals: TimelineMarker[] = [];
    const symptoms: TimelineMarker[] = [];
    const list = this.logs || [];

    list.forEach((log, index) => {
      const at = new Date(log.date);
      if (Number.isNaN(at.getTime())) {
        return;
      }
      const percent = this.percentOfDay(at);
      const timeLabel = at.toLocaleTimeString(this.dateLocale, {
        hour: '2-digit',
        minute: '2-digit'
      });

      if (log.mealLog?.productQuantity?.length) {
        const lines = log.mealLog.productQuantity.map(pq => {
          const name = pq.product?.name || '';
          const amount = this.formatAmount(pq.quantity, pq.unit);
          return amount ? `${name} · ${amount}` : name;
        });
        meals.push({
          key: `meal-${log.mealLog.id || index}-${at.getTime()}`,
          kind: 'meal',
          at,
          percent,
          timeLabel,
          summary: this.translate.instant('timeline.mealSummary', {
            count: lines.length
          }),
          lines
        });
      } else if (log.symptomsLog?.symptoms?.length) {
        const scales = log.symptomsLog.symptoms.map(s => s.symptomScale ?? 0);
        const maxScale = Math.max(...scales, 0);
        const lines = log.symptomsLog.symptoms.map(s => {
          const name = translateSymptomType(this.translate, s.symptomType);
          const scale = translateScale(this.translate, this.normalizeScale(s.symptomScale ?? 0));
          return `${name} · ${scale}`;
        });
        symptoms.push({
          key: `symptom-${log.symptomsLog.id || index}-${at.getTime()}`,
          kind: 'symptom',
          at,
          percent,
          timeLabel,
          summary: this.translate.instant('timeline.symptomSummary', {
            count: lines.length
          }),
          lines,
          severity: this.normalizeScale(maxScale),
          tone: this.severityTone(this.normalizeScale(maxScale))
        });
      }
    });

    meals.sort((a, b) => a.at.getTime() - b.at.getTime());
    symptoms.sort((a, b) => a.at.getTime() - b.at.getTime());
    this.mealMarkers = meals;
    this.symptomMarkers = symptoms;

    if (this.selected) {
      const still =
        [...meals, ...symptoms].find(m => m.key === this.selected!.key) || null;
      this.selected = still;
      if (still?.kind === 'meal') {
        this.windowStartPct = this.clampPct(still.percent + (2 / 24) * 100);
        this.windowEndPct = this.clampPct(still.percent + (6 / 24) * 100);
      } else {
        this.windowStartPct = null;
        this.windowEndPct = null;
      }
    }
  }

  private normalizeScale(scale: number): number {
    if (scale >= 0 && scale <= 5) {
      return scale;
    }
    if (scale >= 6 && scale <= 10) {
      return Math.round(scale / 2);
    }
    return Math.max(0, Math.min(5, scale));
  }

  private percentOfDay(at: Date): number {
    const minutes = at.getHours() * 60 + at.getMinutes() + at.getSeconds() / 60;
    return this.clampPct((minutes / DAY_MINUTES) * 100);
  }

  private clampPct(value: number): number {
    return Math.max(0, Math.min(100, value));
  }

  private severityTone(scale: number): 'calm' | 'mild' | 'hot' {
    if (scale <= 0) return 'calm';
    if (scale < 4) return 'mild';
    return 'hot';
  }

  private formatAmount(quantity: number | string, unit?: Unit): string {
    const qty = quantity ?? '';
    const unitName = translateUnit(this.translate, unit);
    if (!unitName) {
      return `${qty}`;
    }
    const canonical = unit?.name || unitName;
    const compact = canonical
      .replace(/^Milliliter$/i, 'ml')
      .replace(/^Millilitre$/i, 'ml')
      .replace(/^Gram$/i, 'g')
      .replace(/^Kilogram$/i, 'kg')
      .replace(/^Liter$/i, 'L')
      .replace(/^Litre$/i, 'L');
    if (compact !== canonical) {
      return `${qty} ${compact}`;
    }
    return `${qty} ${unitName.toLowerCase()}`;
  }
}
