import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { combineLocalDateTime } from '../services/clock-time';

@Component({
  selector: 'app-date-time-input',
  templateUrl: './date-time-input.component.html',
  styleUrl: './date-time-input.component.css',
  standalone: true,
  imports: [FormsModule, TranslateModule]
})
export class DateTimeInputComponent {
  @Input() date: string;
  @Input() time: string;
  @Output() dateChange = new EventEmitter<string>();
  @Output() timeChange = new EventEmitter<string>();
  @Output() dateTimeChange = new EventEmitter<string>();

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
    if (!this.date || !this.time) {
      return;
    }
    this.dateTimeChange.emit(combineLocalDateTime(this.date, this.time));
  }
}
