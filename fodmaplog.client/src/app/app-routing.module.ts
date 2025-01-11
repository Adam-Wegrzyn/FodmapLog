import { Component } from '@angular/core';
import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AddMealLogComponent } from './add-meal-log/add-meal-log.component';
import { DailyLogComponent } from './daily-log/daily-log.component';
import { AddSymptomsLogComponent } from './add-symptoms-log/add-symptoms-log.component';

const routes: Routes = [
  {
    path: "add-meal-log", component: AddMealLogComponent
  },
  {
    path: "daily-log", component: DailyLogComponent
  },
  {
    path: "add-symptoms-log", component: AddSymptomsLogComponent
  }

  
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
