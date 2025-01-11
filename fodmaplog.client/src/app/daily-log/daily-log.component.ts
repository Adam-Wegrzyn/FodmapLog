import { Component, OnInit } from '@angular/core';
import { FodmapLogService } from '../services/fodmap-log-service';
import { FormBuilder } from '@angular/forms';
import { MealLog } from '../domain/MealLog';
import { DailyLog } from '../domain/DailyLog';
import { SymptomType } from '../domain/SymptomType';
import { SymptomScale } from '../domain/SymptomScale';

@Component({
  selector: 'app-daily-log',
  templateUrl: './daily-log.component.html',
  styleUrl: './daily-log.component.css'
})
export class DailyLogComponent implements OnInit {
  constructor(
    private fodmapLogService: FodmapLogService,
    private fb: FormBuilder
  ) { }

  logs: DailyLog[];
  symptomType = SymptomType;
  symptomScale = SymptomScale;
   currentDate: Date = new Date();
  //currentDate: string = new Date().toLocaleDateString();
  

  ngOnInit(): void {
    this.GetDailyLog(this.currentDate.toISOString());
    console.log(this.logs + 'test2')
    
    ;
  }

  GetDailyLog(date: string) {
    this.fodmapLogService.GetDailyLogsByDate(date).subscribe(
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
  onDateChange(newDate: string): void {
    this.GetDailyLog(newDate)
  }

  decreaseDate(): void {
    this.currentDate.setDate(this.currentDate.getDate() - 1);
    this.GetDailyLog(this.currentDate.toDateString());
  }

  increaseDate(): void {
    this.currentDate.setDate(this.currentDate.getDate() + 1);
    this.GetDailyLog(this.currentDate.toDateString());

}}


