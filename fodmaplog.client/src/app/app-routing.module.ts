import { Component } from '@angular/core';
import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { addMealLogComponent } from './add-meal-log/add-meal-log.component';
import { DailyLogComponent } from './daily-log/daily-log.component';
import { addSymptomsLogComponent } from './add-symptoms-log/add-symptoms-log.component';
import { LogoutComponent } from './logout/logout.component';
import { AudioRecorderComponent } from './audio-recorder/audio-recorder.component';

const routes: Routes = [
  {
    path: "", redirectTo: "/daily-log", pathMatch: "full"
    , 
  },
  {
    path: "add-meal-log", component: addMealLogComponent
  },
  {
    path: "add-meal-log/:id", component: addMealLogComponent
  },
  {
    path: "daily-log", component: DailyLogComponent
  },
  {
    path: "daily-log/:date", component: DailyLogComponent
  },
  {
    path: "add-symptoms-log", component: addSymptomsLogComponent,

  },
  {
    path: "add-symptoms-log/:id", component: addSymptomsLogComponent
  },
  {
    path: "audio-rec", component: AudioRecorderComponent
  },
  {
    path: "logout", component: LogoutComponent
  }

  
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
