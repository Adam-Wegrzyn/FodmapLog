import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { Product } from '../domain/Product';
import { ProductsApiService } from '../services/products-api-service';
import { FormArray, FormBuilder, FormControl, FormGroup } from '@angular/forms';
import { Unit } from '../domain/Unit';
import { getLocaleDateTimeFormat } from '@angular/common';
import { MealLog } from '../domain/MealLog';
import { FodmapLogService } from '../services/fodmap-log-service';
import { ActivatedRoute, Router } from '@angular/router';
import { NgxMaterialTimepickerComponent } from 'ngx-material-timepicker';
import { DateTimeInputComponent } from '../date-time-input/date-time-input.component';


@Component({
  selector: 'app-add-meal-log',
  templateUrl: './add-meal-log.component.html',
  styleUrl: './add-meal-log.component.css'
})
export class addMealLogComponent implements OnInit {

  @ViewChild('closeModalBtn') closeModalBtn: ElementRef;
  @ViewChild('timePicker') timePicker: NgxMaterialTimepickerComponent; 
  form: FormGroup;
  name: string;
  units = Object.keys(Unit).filter(key => isNaN(Number(key))).map(key => ({ label: key, value: Unit[key as keyof typeof Unit] }));
  selectedUnit: string = "1";
  quantityInput: number = 1;
  mealLog: MealLog;
  unit = Unit;
  isNew: boolean = true;
  currDate = new Date().toISOString().split('T')[0];
  currTime = new Date().toLocaleTimeString([], {
    hour: '2-digit',
    minute: '2-digit',
    hour12: true
  });
 //currDate: string = "12:23"


  constructor(private productsApiService: ProductsApiService,
    private fodmapLogService: FodmapLogService,
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router
  ) { }
    
  ngOnInit(): void {
    this.form = this.fb.group({
      id: 0,
      date: new Date(),
      productQuantity: this.fb.array([
      ]),
      totalKcal: 0,
      totalCarbohydrates: 0,
      totalProteins: 0,
      totalFat: 0,
      totalFiber: 0
    })
    this.route.params.subscribe(params => {
      // check if this is existing record
      if (params['id']) {
        this.isNew = false;
        this.fodmapLogService.getMealLogById(params['id']).subscribe(
          data => {
            this.form.patchValue(data);
            this.setProductQuantities(data.productQuantity);
            this.currDate = new Date(data.date).toISOString().split('T')[0];
            this.currTime = new Date(data.date).toLocaleTimeString([], {
              hour: '2-digit',
              minute: '2-digit',
              hour12: false
            });
          },
          () => {},
          () => {
            console.log(this.form.value)
          }
        )
      }
    });
   // this.timePicker.nativeElement.open();


  }
  ngAfterViewInit(){
    if(this.isNew){
      setTimeout(() => {
        this.timePicker.updateTime(this.currDate);
        this.timePicker.open();
      });
    }

  }
  setProductQuantities(productQuantity: any[]) {
    const productQuantityFormGroups = productQuantity.map(productQuantity => this.fb.group(productQuantity));
    const productQuantityFormArray = this.fb.array(productQuantityFormGroups);
    this.form.setControl('productQuantity', productQuantityFormArray);
  }

  products: Product[];
  
  get productQuantityArr(): FormArray {
    return this.form.get('productQuantity') as FormArray;
  }

  searchProduct(){
    this.getProduct();
  }



getProduct(){
    this.productsApiService.getProductsByKeyword(this.name)
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
    console.log(this.productQuantityArr);
  }

  SaveMealLog(): void{
//     const [year, month, day] = this.currDate.split('-');
// const [hour, minute] = this.currTime.split(':');
// const customDate = new Date(+year, +month - 1, +day, +hour, +minute, 0);
   // this.form.controls['date'].setValue(customDate);
    console.log('SaveMealLog');
    console.log(this.mealLog)
    console.log(this.form.value);
    if (this.isNew) {
      this.fodmapLogService.addMealLog(this.form.value).subscribe(
        () => {},
        () => {},
        () => {
          this.router.navigate(['/daily-log', this.currDate]);
        }
      );   
    } 
    else {
      this.fodmapLogService.updateMealLog(this.form.value).subscribe(
        () => {},
        () => {},
        () => {
           this.router.navigate(['/daily-log', this.currDate]);
        }
      );   
    }
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

  cancel(): void{
    this.form.reset();
  }

  onDateTimeChanged(newDate: Date) {
    this.form.controls['date'].setValue(newDate);
  }

}
