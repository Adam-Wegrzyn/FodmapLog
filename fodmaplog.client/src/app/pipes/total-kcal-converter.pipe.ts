import { Pipe, PipeTransform } from '@angular/core';
import { Unit } from '../domain/Unit';

@Pipe({
  name: 'totalKcalConverter'
})
export class TotalKcalConverterPipe implements PipeTransform {

  transform(energyKcal100g: number, quantity: number, unit: string): number {
    if (isNaN(quantity)) {
      return 0;
    }  
    else {
       var result = energyKcal100g * Number(quantity) * Number(Unit[unit as keyof typeof Unit]) / 100;
       return result;
    }
  
  }

}
