import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'totalKcalConverter'
})
export class TotalKcalConverterPipe implements PipeTransform {

  transform(energyKcal100g: number, totalGrams: number): number {
    if (Number.isNaN(totalGrams))  {
      console.log('test')
      return 0;
    }  
    else {
      return energyKcal100g * totalGrams / 100;
    }
  
  }

}
