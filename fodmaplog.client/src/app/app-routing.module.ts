import { Component } from '@angular/core';
import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AddMealLogComponent } from './add-meal-log/add-meal-log.component';

const routes: Routes = [
  {
    path: "add-meal-log",
  component: AddMealLogComponent
  }
  
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
