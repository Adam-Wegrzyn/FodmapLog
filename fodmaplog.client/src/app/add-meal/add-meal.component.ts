import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { Product } from '../domain/Product';
import { ProductsApiService } from '../services/products-api-service';
import { FormArray, FormBuilder, FormGroup } from '@angular/forms';
import { Unit } from '../domain/Unit';

@Component({
  selector: 'app-add-meal',
  templateUrl: './add-meal.component.html',
  styleUrl: './add-meal.component.css'
})
export class AddMealComponent implements OnInit {

  @ViewChild('closeModalBtn') closeModalBtn: ElementRef;
  form: FormGroup;
  name: string;
  units = Object.keys(Unit).filter(key => isNaN(Number(key))).map(key => ({ label: key, value: Unit[key as keyof typeof Unit] }));
  selectedUnit: Unit;
  quantityInput: number;

  constructor(private productsApiService: ProductsApiService,
    private fb: FormBuilder
  ) { }
    
  ngOnInit(): void {
    this.form = this.fb.group({
      id: [null],
      date: [''],
      productQuantity: this.fb.array([]),
      totalKcal: ['']
    })
    // this.form = this.fb.group({
    //   id: [null],
    //   name: [''],
    //   productQuantity: [''],
    //   productQuantityUnit: [''],
    //   servingQuantity: [null],
    //   servingQuantityUnit: [''],
    //   nutriments: this.fb.group({
    //     energyKcal100g: [null],
    //     carbohydrates: [null],
    //     carbohydrates100g: [null],
    //     fat: [null],
    //     fat100g: [null],
    //     protein: [null],
    //     protein100g: [null],
    //     fibre: [null],
    //     servingQuantityUnit: [''],
    //   })
    // })

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

  AddProduct(product: Product, quantity: number){
    this.productQuantityArr.push(this.fb.group({
      product: product,
      quantity: quantity,
      unit: this.selectedUnit,
    }));
    console.log('123213213');
    console.log( 'unit ' + this.selectedUnit)
    console.log(this.productQuantityArr);
    this.quantityInput = 0;
    this.closeModalBtn.nativeElement.click();
    
  }

}
