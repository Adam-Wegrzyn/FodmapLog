import { Component } from '@angular/core';
import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AddMealComponent } from './add-meal/add-meal.component';

const routes: Routes = [
  {
    path: "add-meal",
  component: AddMealComponent
  }
  
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
