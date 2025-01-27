import { HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { addMealLogComponent } from './add-meal-log/add-meal-log.component';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { TotalKcalConverterPipe } from './pipes/total-kcal-converter.pipe';
import { DailyLogComponent } from './daily-log/daily-log.component';
import { addSymptomsLogComponent } from './add-symptoms-log/add-symptoms-log.component';
import { AddLogBaseComponent } from './add-log-base/add-log-base.component';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { CommonModule } from '@angular/common';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { NgxMaterialTimepickerModule } from 'ngx-material-timepicker';
import { DateTimeInputComponent } from './date-time-input/date-time-input.component';

@NgModule({
  declarations: [
    AppComponent,
    addMealLogComponent,
    TotalKcalConverterPipe,
    DailyLogComponent,
    addSymptomsLogComponent,
    AddLogBaseComponent,
    //DateTimeInputComponent,
  ],
  imports: [
    BrowserModule,
     HttpClientModule,
    AppRoutingModule,
    FormsModule,
    ReactiveFormsModule,
    CommonModule,
    FontAwesomeModule,
    NgxMaterialTimepickerModule,
    DateTimeInputComponent
  ],
  providers: [
    provideAnimationsAsync('noop')
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
