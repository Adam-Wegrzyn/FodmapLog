import { Component, OnInit } from '@angular/core';
import { FodmapLogService } from '../services/fodmap-log-service';
import { FormBuilder } from '@angular/forms';
import { MealLog } from '../domain/MealLog';
import { DailyLog } from '../domain/DailyLog';

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

  ngOnInit(): void {
    this.GetDailyLog();
    ;
  }

  GetDailyLog() {
    this.fodmapLogService.GetDailyLogsByDate(new Date().toISOString()).subscribe((data) => {
      this.logs = data,
      () => console.log(),
      () => console.log(this.logs)
    }
  
  )
    
  }

}


