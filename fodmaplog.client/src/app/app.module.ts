import { HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { AddMealLogComponent } from './add-meal-log/add-meal-log.component';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { TotalKcalConverterPipe } from './pipes/total-kcal-converter.pipe';
import { DailyLogComponent } from './daily-log/daily-log.component';

@NgModule({
  declarations: [
    AppComponent,
    AddMealLogComponent,
    TotalKcalConverterPipe,
    DailyLogComponent,
  ],
  imports: [
    BrowserModule,
     HttpClientModule,
    AppRoutingModule,
    FormsModule,
    ReactiveFormsModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
