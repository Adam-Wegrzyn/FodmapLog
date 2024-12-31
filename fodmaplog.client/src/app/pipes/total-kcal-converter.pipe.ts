import { Pipe, PipeTransform } from '@angular/core';
import { Unit } from '../domain/Unit';

@Pipe({
  name: 'totalKcalConverter'
})
export class TotalKcalConverterPipe implements PipeTransform {

  transform(energyKcal100g: number = 0, quantity: number, unit: string): number {
    if (isNaN(quantity)) {
      return 0;
    }  
    else {
       var result = energyKcal100g * Number(quantity) * Number(unit) / 100;
       return Number(result.toFixed(0));
    }
  
  }

}
