import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { FodmapLogService } from '../services/fodmap-log-service';
import { FormBuilder } from '@angular/forms';
import { MealLog } from '../domain/MealLog';
import { DailyLog } from '../domain/DailyLog';
import { SymptomType } from '../domain/SymptomType';
import { SymptomScale } from '../domain/SymptomScale';
import { faCircleChevronRight, faCircleChevronLeft, faPlusCircle } from '@fortawesome/free-solid-svg-icons';
import { ActivatedRoute, Router } from '@angular/router';
import { OpenAiService } from '../services/openAi-service';
import { DailyLogUI } from '../domain/DailyLogUI';
import { SymptomsLog } from '../domain/SymptomsLog';
import { MealLogTransferService } from '../services/meal-log-transfer.service';
@Component({
  selector: 'app-daily-log',
  templateUrl: './daily-log.component.html',
  styleUrl: './daily-log.component.css'
})
export class DailyLogComponent implements OnInit {
  constructor(
    private fodmapLogService: FodmapLogService,
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private openAiService: OpenAiService,
    private cdr: ChangeDetectorRef ,
    private mealLogTransferService: MealLogTransferService
  ) { }

  logs: DailyLogUI[];
  showReviewModal = false;
  tempLogs: DailyLog[];
  symptomType = SymptomType;
  symptomScale = SymptomScale;
  currentDate: Date = new Date();
  setDateCalendar: string = new Date().toISOString().split('T')[0];
  //setDateCalendar = "2025-01-03";
  faCircleChevronRight = faCircleChevronRight;
  faCircleChevronLeft = faCircleChevronLeft;
  faPlusCircle = faPlusCircle;
  symptomScaleValues = Object.values(SymptomScale);
  
  

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      if (params['date']) {
        this.setDateCalendar = params['date'];
        console.log(params);
      }
    })
    console.log(this.getSymptomScaleValue('Low'))
    
    this.GetDailyLog(this.setDateCalendar);
    //this.GetDailyLogFromAI()

    ;
  }
  
  editPendingMealLog(mealLog: MealLog): void {
    this.mealLogTransferService.mealLog = mealLog;
    this.router.navigate(
      ['/add-meal-log'],
      { queryParams: {isPending: true} }
    );
  }
  GenerateMealLogFromAI(transcription: string) {
    this.openAiService.generateMealLogFromAI(transcription).subscribe(
      data => {
        for(const mealLog of data){
        const newDailyLog: DailyLogUI = new DailyLog(0, mealLog.date, mealLog.mealLog, mealLog.symptomsLog);
        newDailyLog.isPending = true;
        this.logs = [newDailyLog, ...this.logs];
        }
        this.tempLogs = data;
        console.log(this.logs);
        this.showReviewModal = true;
        this.cdr.detectChanges();
      }
      , error => {
        console.log(error);
      }
      , () => {
        console.log(this.logs);
      }
    )
  }
  getSymptomScaleValue(key: string): number {
    return SymptomScale[key as keyof typeof SymptomScale];
  }
  GetDailyLog(date: string) {
    this.fodmapLogService.getDailyLogsByDate(date).subscribe(
      data => {

        this.logs = data;
      },
      error => {
        console.log(error);
      },
      () => {
        console.log(this.logs);
      }

  
  )
}

  isMealLog(log: DailyLog): boolean {
    return log.mealLog != undefined && log.mealLog != null;
  }
  updateCalendar(date:  Date): void {
    this.setDateCalendar = this.getOnlyStringDate(date);
  }
    
 
  onDateChange(newDate: string): void {
    this.GetDailyLog(newDate)
  }

  decreaseDate(): void {
    this.currentDate.setDate(this.currentDate.getDate() - 1);
    this.setDateCalendar = this.getOnlyStringDate(this.currentDate);
    this.GetDailyLog(this.setDateCalendar);
  }

  increaseDate(): void {
    this.currentDate.setDate(this.currentDate.getDate() + 1);
    this.setDateCalendar = this.getOnlyStringDate(this.currentDate);
    this.GetDailyLog(this.currentDate.toDateString());
  
}
getOnlyStringDate(date: Date): string { 
  return date.toISOString().split('T')[0];
}

saveMealLog(mealLog: DailyLogUI): void {
}



// SaveReviewLog() {
//     if (!this.tempLogs) return;
//     if (this.tempLogs.mealLog) {
//       this.fodmapLogService.saveMealLog(this.tempLog.mealLog).subscribe(
//         () => {
//           this.showReviewModal = false;
//           this.tempLog = null;
//           this.GetDailyLog(this.setDateCalendar);
//         },
//         error => {
//           console.log(error);
//         }
//       );
//     }
//     // Add similar logic for symptomsLog if needed
//   }

  // cancelReviewLog() {
  //   this.showReviewModal = false;
  //   this.tempLog = null;
  // }
}

