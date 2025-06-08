import { Injectable } from '@angular/core';
import { MealLog } from '../domain/MealLog';

@Injectable({
  providedIn: 'root'
})
export class MealLogTransferService {
  mealLog: MealLog | null = null;
  constructor() { }
}
