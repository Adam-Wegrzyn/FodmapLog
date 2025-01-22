import { Component } from '@angular/core';
import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AddMealLogComponent } from './add-meal-log/add-meal-log.component';
import { DailyLogComponent } from './daily-log/daily-log.component';
import { AddSymptomsLogComponent } from './add-symptoms-log/add-symptoms-log.component';

const routes: Routes = [
  {
    path: "", redirectTo: "/daily-log", pathMatch: "full"
  },
  {
    path: "add-meal-log", component: AddMealLogComponent
  },
  {
    path: "add-meal-log/:id", component: AddMealLogComponent
  },
  {
    path: "daily-log", component: DailyLogComponent
  },
  {
    path: "daily-log/:date", component: DailyLogComponent
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
