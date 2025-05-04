import { Component, OnInit } from '@angular/core';
import { FodmapLogService } from '../services/fodmap-log-service';
import { FormBuilder } from '@angular/forms';
import { MealLog } from '../domain/MealLog';
import { DailyLog } from '../domain/DailyLog';
import { SymptomType } from '../domain/SymptomType';
import { SymptomScale } from '../domain/SymptomScale';
import { faCircleChevronRight, faCircleChevronLeft, faPlusCircle } from '@fortawesome/free-solid-svg-icons';
import { ActivatedRoute } from '@angular/router';
import { MsalService } from '@azure/msal-angular';
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
    private msalService: MsalService,
  ) { }

  logs: DailyLog[];
  symptomType = SymptomType;
  symptomScale = SymptomScale;
  currentDate: Date = new Date();
  setDateCalendar: string = new Date().toISOString().split('T')[0];
  //setDateCalendar = "2025-01-03";
  faCircleChevronRight = faCircleChevronRight;
  faCircleChevronLeft = faCircleChevronLeft;
  faPlusCircle = faPlusCircle;
  
  

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      if (params['date']) {
        this.setDateCalendar = params['date'];
        console.log(params);
      }
    })

    
    this.GetDailyLog(this.setDateCalendar);
    console.log(this.logs + 'test2')
    
    ;
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
}

