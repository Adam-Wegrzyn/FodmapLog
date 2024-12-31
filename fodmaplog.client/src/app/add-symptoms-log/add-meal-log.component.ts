import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { Product } from '../domain/Product';
import { ProductsApiService } from '../services/products-api-service';
import { FormArray, FormBuilder, FormGroup } from '@angular/forms';
import { Unit } from '../domain/Unit';
import { getLocaleDateTimeFormat } from '@angular/common';
import { MealLog } from '../domain/MealLog';
import { FodmapLogService } from '../services/fodmap-log-service';

@Component({
  selector: 'app-add-meal-log',
  templateUrl: './add-meal-log.component.html',
  styleUrl: './add-meal-log.component.css'
})
export class AddMealLogComponent implements OnInit {

  @ViewChild('closeModalBtn') closeModalBtn: ElementRef;
  form: FormGroup;
  name: string;
  units = Object.keys(Unit).filter(key => isNaN(Number(key))).map(key => ({ label: key, value: Unit[key as keyof typeof Unit] }));
  selectedUnit: string = "1";
  quantityInput: number = 1;
  mealLog: MealLog;
  unit = Unit;

  constructor(private productsApiService: ProductsApiService,
    private fodmapLogService: FodmapLogService,
    private fb: FormBuilder
  ) { }
    
  ngOnInit(): void {
    this.form = this.fb.group({
      id: 0,
      date: new Date(),
      productQuantity: this.fb.array([]),
      totalKcal: 0,
      totalCarbohydrates: 0,
      totalProteins: 0,
      totalFat: 0,
      totalFiber: 0
    })
        console.log(this.products)
    }

  products: Product[];
  
  get productQuantityArr(): FormArray {
    return this.form.get('productQuantity') as FormArray;
  }
  SearchProduct(){
    this.GetProduct();
  }



GetProduct(){
    this.productsApiService.GetProductsByKeyword(this.name)
    .subscribe((results: Product[]) => {
      this.products = results;
      console.log(results);
      console.log(this.products[0].servingQuantity)
    });
  }

  AddProduct(product: Product, quantity: number): void{
    this.productQuantityArr.push(this.fb.group({
      product: product,
      quantity: quantity,
       unit: Number(this.selectedUnit),
       totalGrams: quantity * Number(this.selectedUnit),
    }));
    console.log(product)
    console.log(this.productQuantityArr);
    this.quantityInput = 0;
    this.closeModalBtn.nativeElement.click();
    
  }

  DeleteProduct(index: number): void{
    this.productQuantityArr.removeAt(index);
  }

  SaveMealLog(): void{
    this.mealLog = this.form.value;
    
    console.log('SaveMealLog');
    console.log(this.mealLog)
    console.log(this.form.value);
    this.fodmapLogService.AddMealLog(this.mealLog).subscribe();
   
  }
  
  TotalKcalConverter(energyKcal100g: number, totalGrams: number): number {
    if (Number.isNaN(totalGrams))  {
      console.log('test')
      return 0;
    }  
    else {
      return energyKcal100g * totalGrams / 100;
    }
  }

}
