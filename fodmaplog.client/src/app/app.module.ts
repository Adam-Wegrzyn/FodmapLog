import { HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { AddMealComponent } from './add-meal/add-meal.component';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { TotalKcalConverterPipe } from './pipes/total-kcal-converter.pipe';

@NgModule({
  declarations: [
    AppComponent,
    AddMealComponent,
    TotalKcalConverterPipe,
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
