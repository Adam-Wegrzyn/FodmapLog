import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { BrowserModule } from '@angular/platform-browser';
import {NgxMaterialTimepickerModule } from 'ngx-material-timepicker';


@Component({
  selector: 'app-date-time-input',
  templateUrl: './date-time-input.component.html',
  styleUrl: './date-time-input.component.css',
  standalone: true,
  imports: [NgxMaterialTimepickerModule, FormsModule]
})
export class DateTimeInputComponent {
  @Input() date: string;
  @Input() time: string;
  @Output() dateChange = new EventEmitter<string>();
  @Output() timeChange = new EventEmitter<string>();
  @Output() dateTimeChange = new EventEmitter<Date>();

  onDateChange(newDate: string) {
    this.date = newDate;
    this.dateChange.emit(this.date);
    this.emitDateTimeChange();
  }

  onTimeChange(newTime: string) {
    this.time = newTime;
    this.timeChange.emit(this.time);
    this.emitDateTimeChange();
  }
  
  private emitDateTimeChange() {
    const [year, month, day] = this.date.split('-');
    const [hour, minute] = this.time.split(':');
    const customDate = new Date(Date.UTC(+year, +month - 1, +day, +hour, +minute, 0));
    this.dateTimeChange.emit(customDate);
  }

}
