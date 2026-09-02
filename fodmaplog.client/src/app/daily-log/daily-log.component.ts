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

  readonly emptyExample =
    '“I had oatmeal with milk at 8, then felt bloated around 10.”';

  constructor(
    private fodmapLogService: FodmapLogService,
    private route: ActivatedRoute,
    private router: Router,
    private openAiService: OpenAiService,
    private cdr: ChangeDetectorRef,
    private mealLogTransferService: MealLogTransferService,
    private symptomsLogTransferService: SymptomsLogTransferService
  ) {}

  ngOnInit(): void {
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
    this.openAiService.generateMealLogFromAI(transcription).pipe(
      finalize(() => {
        this.isUnderstanding = false;
        this.cdr.detectChanges();
      })
    ).subscribe({
      next: (data) => {
        const events = Array.isArray(data) ? data : [];
        if (events.length === 0) {
          this.aiError = 'No meals or symptoms found in that recording. Try again.';
          return;
        }

        this.truncatedPendingCount = Math.max(0, events.length - REVIEW_SOFT_CAP);
        const capped = events.slice(0, REVIEW_SOFT_CAP);

        // Replace previous pending AI batch with this one
        this.logs = (this.logs || []).filter(l => !l.isPending);
        for (const item of capped) {
          const newDailyLog: DailyLogUI = new DailyLog(
            0,
            item.date,
            item.mealLog,
            item.symptomsLog
          );
          newDailyLog.isPending = true;
          this.logs = [newDailyLog, ...this.logs];
        }
        this.showReviewSheet = true;
        this.transcriptExpanded = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.aiError = 'Could not understand that recording. Please try again.';
      }
    });
  }

  GetDailyLog(date: string): void {
    this.fodmapLogService.getDailyLogsByDate(date).subscribe({
      next: (data) => {
        const pending = this.pendingLogs;
        this.logs = [...pending, ...(data || [])];
      },
      error: (error) => console.error(error)
    });
  }

  isMealLog(log: DailyLog): boolean {
    return log.mealLog != undefined && log.mealLog != null;
  }

  /** Friendly amount like "1 bowl" / "200 ml" (food-diary style). */
  formatAmount(quantity: number | string, unitName?: string): string {
    const qty = quantity ?? '';
    const unit = (unitName || '').trim();
    if (!unit) {
      return `${qty}`;
    }
    const compact = unit
      .replace(/^Milliliter$/i, 'ml')
      .replace(/^Millilitre$/i, 'ml')
      .replace(/^Gram$/i, 'g')
      .replace(/^Kilogram$/i, 'kg')
      .replace(/^Liter$/i, 'L')
      .replace(/^Litre$/i, 'L');
    const lower = compact.length <= 3 && compact === compact.toUpperCase()
      ? compact
      : compact.toLowerCase();
    return `${qty} ${lower}`;
  }

  severityLabel(scale: number): string {
    return this.symptomScale[scale] ?? `${scale}`;
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

  closeReviewSheet(): void {
    this.showReviewSheet = false;
  }

  discardAllPending(): void {
    this.logs = this.logs.filter(l => !l.isPending);
    this.showReviewSheet = false;
    this.reviewError = null;
    this.truncatedPendingCount = 0;
  }

  reRecord(): void {
    this.discardAllPending();
    this.aiError = null;
    setTimeout(() => this.audioRecorder?.startRecording(), 0);
  }

  saveAllPending(): void {
    const pending = this.pendingLogs;
    if (pending.length === 0 || this.isSavingAll) {
      return;
    }

    this.isSavingAll = true;
    this.reviewError = null;

    const requests: Observable<unknown>[] = pending.map(log => {
      if (this.isMealLog(log) && log.mealLog) {
        return this.fodmapLogService.addMealLog(log.mealLog).pipe(
          catchError(err => {
            console.error(err);
            return of({ __failed: true });
          })
        );
      }
      if (log.symptomsLog) {
        return this.fodmapLogService.addSymptomsLog(log.symptomsLog).pipe(
          catchError(err => {
            console.error(err);
            return of({ __failed: true });
          })
        );
      }
      return of({ __failed: true });
    });

    forkJoin(requests).pipe(
      finalize(() => {
        this.isSavingAll = false;
        this.cdr.detectChanges();
      })
    ).subscribe(results => {
      const failed = results.filter(r => (r as { __failed?: boolean })?.__failed).length;
      if (failed > 0 && failed === results.length) {
        this.reviewError = 'Could not save events. Check your connection and try again.';
        return;
      }
      if (failed > 0) {
        this.reviewError = `Saved ${results.length - failed} of ${results.length} events. Retry failed ones.`;
      }
      this.logs = this.logs.filter(l => !l.isPending);
      this.showReviewSheet = false;
      this.truncatedPendingCount = 0;
      this.GetDailyLog(this.setDateCalendar);
    });
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
