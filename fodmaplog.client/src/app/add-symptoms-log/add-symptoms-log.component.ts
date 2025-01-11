import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { Product } from '../domain/Product';
import { ProductsApiService } from '../services/products-api-service';
import { FormArray, FormBuilder, FormGroup } from '@angular/forms';
import { Unit } from '../domain/Unit';
import { FodmapLogService } from '../services/fodmap-log-service';
import { Symptom } from '../domain/Symptom';
import { SymptomsLog } from '../domain/SymptomsLog';
import { SymptomType } from '../domain/SymptomType';
import { SymptomScale } from '../domain/SymptomScale';

@Component({
  selector: 'app-add-symptoms-log',
  templateUrl: './add-symptoms-log.component.html',
  styleUrl: './add-symptoms-log.component.css'
})
export class AddSymptomsLogComponent implements OnInit {

  @ViewChild('closeModalBtn') closeModalBtn: ElementRef;
  form: FormGroup;
  name: string;
  symptomTypes = Object.keys(SymptomType).filter(key => isNaN(Number(key))).map(key => ({ label: key, value: SymptomType[key as keyof typeof SymptomType] }));
  selectedUnit: string = "1";
  quantityInput: number = 1;
  symptomsLog: SymptomsLog;
  symptomType = SymptomType;
  symptomRange: number;
  symptomScale = SymptomScale

  constructor(private productsApiService: ProductsApiService,
    private fodmapLogService: FodmapLogService,
    private fb: FormBuilder
  ) { }
    
  ngOnInit(): void {
    this.form = this.fb.group({
      id: 0,
      date: new Date(),
      symptoms: this.fb.array([

      ]),
    })
        console.log(this.products)
        console.log(this.symptomType[2])
    }
   

  products: Product[];
  
  get symptomsArr(): FormArray {
    return this.form.get('symptoms') as FormArray;
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

  AddSymptom(symptomType: SymptomType): void{
    this.symptomsArr.push(this.fb.group({
      symptomType: symptomType,
      symptomScale: 0
    }));
    // console.log(this.symptomsArr);
    this.quantityInput = 0;
    console.log(this.symptomType)
    
  }

  DeleteSymptom(index: number): void{
    this.symptomsArr.removeAt(index);
  }

  SaveSymptomsLog(): void{
    this.symptomsLog = this.form.value;
    
    console.log(this.symptomsLog)
    this.fodmapLogService.AddSymptomsLog(this.symptomsLog).subscribe();
   
  }
  

}
