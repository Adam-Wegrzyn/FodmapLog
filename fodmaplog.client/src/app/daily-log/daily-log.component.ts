import { ChangeDetectorRef, Component, OnInit, ViewChild } from '@angular/core';
import { FodmapLogService } from '../services/fodmap-log-service';
import { MealLog } from '../domain/MealLog';
import { DailyLog } from '../domain/DailyLog';
import { SymptomScale } from '../domain/SymptomScale';
import {
  faCircleChevronRight,
  faCircleChevronLeft,
  faPlusCircle,
  faChevronDown,
  faChevronUp
} from '@fortawesome/free-solid-svg-icons';
import { ActivatedRoute, Router } from '@angular/router';
import { OpenAiService } from '../services/openAi-service';
import { DailyLogUI } from '../domain/DailyLogUI';
import { SymptomsLog } from '../domain/SymptomsLog';
import { MealLogTransferService } from '../services/meal-log-transfer.service';
import { SymptomsLogTransferService } from '../services/symptoms-log-transfer.service';
import { AudioRecorderComponent } from '../audio-recorder/audio-recorder.component';
import { TranslateService } from '@ngx-translate/core';
import { LanguageService } from '../services/language.service';
import { Unit } from '../domain/Unit';
import { translateScale, translateSymptomType, translateUnit } from '../services/reference-i18n';
import { forkJoin, Observable, of } from 'rxjs';
import { catchError, finalize } from 'rxjs/operators';

const REVIEW_SOFT_CAP = 25;
const GROUP_THRESHOLD = 8;

type TimeOfDayGroup = 'Morning' | 'Afternoon' | 'Evening' | 'Night';

interface PendingGroup {
  label: TimeOfDayGroup;
  logs: DailyLogUI[];
}

@Component({
  selector: 'app-daily-log',
  templateUrl: './daily-log.component.html',
  styleUrl: './daily-log.component.css'
})
export class DailyLogComponent implements OnInit {
  @ViewChild(AudioRecorderComponent) audioRecorder?: AudioRecorderComponent;

  logs: DailyLogUI[] = [];
  showReviewSheet = false;
  transcriptExpanded = false;
  lastTranscript = '';
  isUnderstanding = false;
  isSavingAll = false;
  reviewError: string | null = null;
  aiError: string | null = null;
  truncatedPendingCount = 0;

  symptomScale = SymptomScale;
  currentDate: Date = new Date();
  setDateCalendar: string = new Date().toISOString().split('T')[0];
  faCircleChevronRight = faCircleChevronRight;
  faCircleChevronLeft = faCircleChevronLeft;
  faPlusCircle = faPlusCircle;
  faChevronDown = faChevronDown;
  faChevronUp = faChevronUp;

  constructor(
    private fodmapLogService: FodmapLogService,
    private route: ActivatedRoute,
    private router: Router,
    private openAiService: OpenAiService,
    private cdr: ChangeDetectorRef,
    private mealLogTransferService: MealLogTransferService,
    private symptomsLogTransferService: SymptomsLogTransferService,
    private translate: TranslateService,
    private language: LanguageService
  ) {}

  get dateLocale(): string {
    return this.language.dateLocale;
  }

  ngOnInit(): void {
    this.translate.onLangChange.subscribe(() => this.cdr.detectChanges());
    this.route.params.subscribe(params => {
      if (params['date']) {
        this.setDateCalendar = params['date'];
        this.currentDate = new Date(params['date'] + 'T12:00:00');
      }
      this.GetDailyLog(this.setDateCalendar);
    });
  }

  get savedLogs(): DailyLogUI[] {
    return (this.logs || []).filter(l => !l.isPending);
  }

  get pendingLogs(): DailyLogUI[] {
    return (this.logs || []).filter(l => l.isPending);
  }

  get pendingCount(): number {
    return this.pendingLogs.length;
  }

  get isEmptyDay(): boolean {
    return this.savedLogs.length === 0 && !this.showReviewSheet && !this.isUnderstanding;
  }

  get useTimeGroups(): boolean {
    return this.pendingCount > GROUP_THRESHOLD;
  }

  get pendingGroups(): PendingGroup[] {
    if (!this.useTimeGroups) {
      return [];
    }
    const order: TimeOfDayGroup[] = ['Morning', 'Afternoon', 'Evening', 'Night'];
    const map = new Map<TimeOfDayGroup, DailyLogUI[]>();
    for (const label of order) {
      map.set(label, []);
    }
    for (const log of this.pendingLogs) {
      map.get(this.timeOfDay(log.date))!.push(log);
    }
    return order
      .map(label => ({ label, logs: map.get(label)! }))
      .filter(g => g.logs.length > 0);
  }

  editPendingMealLog(mealLog: MealLog): void {
    this.mealLogTransferService.mealLog = mealLog;
    this.router.navigate(['/add-meal-log'], { queryParams: { isPending: true } });
  }

  editPendingSymptomsLog(symptomsLog: SymptomsLog): void {
    this.symptomsLogTransferService.symptomsLog = symptomsLog;
    this.router.navigate(['/add-symptoms-log'], { queryParams: { isPending: true } });
  }

  onTranscript(transcription: string): void {
    this.lastTranscript = transcription;
    this.aiError = null;
    this.GenerateMealLogFromAI(transcription);
  }

  GenerateMealLogFromAI(transcription: string): void {
    if (!transcription?.trim()) {
      return;
    }
    this.isUnderstanding = true;
    this.reviewError = null;
    this.openAiService.generateMealLogFromAI(transcription, this.language.currentLang).pipe(
      finalize(() => {
        this.isUnderstanding = false;
        this.cdr.detectChanges();
      })
    ).subscribe({
      next: (data) => {
        const events = Array.isArray(data) ? data : [];
        if (events.length === 0) {
          this.aiError = this.translate.instant('daily.aiEmpty');
          return;
        }

        this.truncatedPendingCount = Math.max(0, events.length - REVIEW_SOFT_CAP);
        const capped = events.slice(0, REVIEW_SOFT_CAP);

        // Replace previous pending AI batch with this one
        this.logs = (this.logs || []).filter(l => !l.isPending);
        for (const item of capped) {
          // AI often invents its own calendar day (prompt examples / model "today").
          // Review UI only shows time, so pin events to the day the user is viewing.
          const pinnedDate = this.pinToSelectedDay(item?.date);
          const mealLog = item?.mealLog
            ? { ...item.mealLog, date: this.pinToSelectedDay(item.mealLog.date || item.date) }
            : null;
          const symptomsLog = item?.symptomsLog
            ? { ...item.symptomsLog, date: this.pinToSelectedDay(item.symptomsLog.date || item.date) }
            : null;
          const newDailyLog: DailyLogUI = new DailyLog(
            0,
            pinnedDate,
            mealLog as MealLog,
            symptomsLog as SymptomsLog
          );
          newDailyLog.isPending = true;
          this.logs = [newDailyLog, ...this.logs];
        }
        this.showReviewSheet = true;
        this.transcriptExpanded = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.aiError = this.translate.instant('daily.aiFail');
      }
    });
  }

  GetDailyLog(date: string): void {
    this.fodmapLogService.getDailyLogsByDate(date).subscribe({
      next: (data) => {
        const pending = this.pendingLogs;
        this.logs = [...pending, ...(data || [])];
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error(error);
        this.aiError = this.translate.instant('daily.refreshFail');
        this.cdr.detectChanges();
      }
    });
  }

  isMealLog(log: DailyLog): boolean {
    return !!log?.mealLog && (log.symptomsLog == null || log.symptomsLog === undefined);
  }

  /** Friendly amount like "1 bowl" / "200 ml" (food-diary style). */
  formatAmount(quantity: number | string, unit?: Unit | string): string {
    const qty = quantity ?? '';
    let unitName = '';
    if (typeof unit === 'string') {
      unitName = unit;
    } else if (unit) {
      unitName = translateUnit(this.translate, unit);
    }
    const trimmed = (unitName || '').trim();
    if (!trimmed) {
      return `${qty}`;
    }
    const canonical = typeof unit === 'object' && unit?.name ? unit.name : trimmed;
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
    const lower = trimmed.length <= 3 && trimmed === trimmed.toUpperCase()
      ? trimmed
      : trimmed.toLowerCase();
    return `${qty} ${lower}`;
  }

  symptomLabel(type: { id?: number; name?: string } | null | undefined): string {
    return translateSymptomType(this.translate, type);
  }

  severityLabel(scale: number): string {
    return translateScale(this.translate, scale);
  }

  severityTone(scale: number): 'calm' | 'mild' | 'hot' {
    if (scale <= 0) return 'calm';
    if (scale < 4) return 'mild';
    return 'hot';
  }

  onDateChange(newDate: string): void {
    this.currentDate = new Date(newDate + 'T12:00:00');
    this.GetDailyLog(newDate);
  }

  decreaseDate(): void {
    this.currentDate.setDate(this.currentDate.getDate() - 1);
    this.setDateCalendar = this.getOnlyStringDate(this.currentDate);
    this.GetDailyLog(this.setDateCalendar);
  }

  increaseDate(): void {
    this.currentDate.setDate(this.currentDate.getDate() + 1);
    this.setDateCalendar = this.getOnlyStringDate(this.currentDate);
    this.GetDailyLog(this.setDateCalendar);
  }

  getOnlyStringDate(date: Date): string {
    return date.toISOString().split('T')[0];
  }

  discardPending(log: DailyLogUI, event?: Event): void {
    event?.preventDefault();
    event?.stopPropagation();
    this.logs = this.logs.filter(l => l !== log);
    if (this.pendingCount === 0) {
      this.showReviewSheet = false;
    }
  }

  deleteLog(log: DailyLogUI, event?: Event): void {
    event?.preventDefault();
    event?.stopPropagation();
    if (log.isPending) {
      this.discardPending(log, event);
    }
  }

  closeReviewSheet(event?: Event): void {
    event?.preventDefault();
    event?.stopPropagation();
    // Dismiss the AI draft entirely — same as abandoning the review sheet.
    this.discardAllPending();
    this.cdr.detectChanges();
  }

  discardAllPending(): void {
    this.logs = this.logs.filter(l => !l.isPending);
    this.showReviewSheet = false;
    this.reviewError = null;
    this.truncatedPendingCount = 0;
  }

  reRecord(event?: Event): void {
    event?.preventDefault();
    event?.stopPropagation();
    this.discardAllPending();
    this.aiError = null;
    this.cdr.detectChanges();
    setTimeout(() => this.audioRecorder?.startRecording(), 0);
  }

  saveAllPending(event?: Event): void {
    event?.preventDefault();
    event?.stopPropagation();
    const pending = this.pendingLogs;
    if (pending.length === 0 || this.isSavingAll) {
      return;
    }

    this.isSavingAll = true;
    this.reviewError = null;
    this.cdr.detectChanges();

    const saveRequests = pending.map((log, index) => {
      let req: Observable<unknown>;
      if (this.isMealLog(log) && log.mealLog) {
        req = this.fodmapLogService.addMealLog(this.prepareMealLogForSave(log.mealLog, log.date));
      } else if (log.symptomsLog) {
        req = this.fodmapLogService.addSymptomsLog(this.prepareSymptomsLogForSave(log.symptomsLog, log.date));
      } else {
        return of({ __failed: true, __index: index });
      }
      return req.pipe(
        catchError(err => {
          console.error(err);
          return of({ __failed: true, __index: index });
        })
      );
    });

    forkJoin(saveRequests).pipe(
      finalize(() => {
        this.isSavingAll = false;
        this.cdr.detectChanges();
      })
    ).subscribe(results => {
      const failedIndexes = new Set(
        results
          .map((r, i) => ((r as { __failed?: boolean })?.__failed ? i : -1))
          .filter(i => i >= 0)
      );
      const failed = failedIndexes.size;
      if (failed > 0 && failed === results.length) {
        this.reviewError = this.translate.instant('daily.saveFail');
        this.cdr.detectChanges();
        return;
      }

      // Drop only the ones that saved; keep failed pending for retry.
      this.logs = this.logs.filter(l => {
        if (!l.isPending) {
          return true;
        }
        const idx = pending.indexOf(l);
        return failedIndexes.has(idx);
      });

      if (failed > 0) {
        this.reviewError = this.translate.instant('daily.savePartial', {
          saved: results.length - failed,
          total: results.length
        });
        this.showReviewSheet = true;
        this.cdr.detectChanges();
        return;
      }

      this.showReviewSheet = false;
      this.truncatedPendingCount = 0;
      this.reviewError = null;
      this.GetDailyLog(this.setDateCalendar);
    });
  }

  /**
   * Keep AI time-of-day but force the calendar day the user is viewing.
   * Avoids silent "saved on another day" when the model invents dates.
   */
  private pinToSelectedDay(isoOrDate: string | Date | null | undefined): string {
    const day = this.setDateCalendar || new Date().toISOString().split('T')[0];
    let hours = 12;
    let minutes = 0;
    let seconds = 0;
    if (isoOrDate) {
      const parsed = new Date(isoOrDate);
      if (!Number.isNaN(parsed.getTime())) {
        hours = parsed.getHours();
        minutes = parsed.getMinutes();
        seconds = parsed.getSeconds();
      }
    }
    const hh = String(hours).padStart(2, '0');
    const mm = String(minutes).padStart(2, '0');
    const ss = String(seconds).padStart(2, '0');
    return `${day}T${hh}:${mm}:${ss}`;
  }

  private prepareMealLogForSave(mealLog: MealLog, fallbackDate: string): MealLog {
    const date = this.pinToSelectedDay(mealLog?.date || fallbackDate);
    return {
      id: mealLog?.id ?? 0,
      date,
      productQuantity: (mealLog?.productQuantity || []).map(pq => ({
        id: pq?.id ?? 0,
        quantity: pq?.quantity ?? 1,
        product: {
          id: pq?.product?.id ?? 0,
          name: (pq?.product?.name || 'Unknown').trim() || 'Unknown'
        },
        unit: {
          id: pq?.unit?.id ?? 0,
          name: (pq?.unit?.name || 'Piece').trim() || 'Piece'
        }
      }))
    } as MealLog;
  }

  private prepareSymptomsLogForSave(symptomsLog: SymptomsLog, fallbackDate: string): SymptomsLog {
    const date = this.pinToSelectedDay(symptomsLog?.date || fallbackDate);
    return {
      id: symptomsLog?.id ?? 0,
      date,
      symptoms: (symptomsLog?.symptoms || []).map(s => ({
        id: s?.id ?? 0,
        symptomScale: s?.symptomScale ?? 0,
        symptomType: {
          id: s?.symptomType?.id ?? 0,
          name: (s?.symptomType?.name || 'Unknown').trim() || 'Unknown'
        }
      }))
    } as SymptomsLog;
  }

  toggleTranscript(): void {
    this.transcriptExpanded = !this.transcriptExpanded;
  }

  private timeOfDay(dateValue: string): TimeOfDayGroup {
    const hour = new Date(dateValue).getHours();
    if (hour >= 5 && hour < 12) return 'Morning';
    if (hour >= 12 && hour < 17) return 'Afternoon';
    if (hour >= 17 && hour < 21) return 'Evening';
    return 'Night';
  }
}
