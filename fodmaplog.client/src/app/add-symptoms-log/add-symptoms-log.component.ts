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
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-add-symptoms-log',
  templateUrl: './add-symptoms-log.component.html',
  styleUrl: './add-symptoms-log.component.css'
})
export class addSymptomsLogComponent implements OnInit {

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
  currDate = new Date().toISOString().split('T')[0];
  currTime = new Date().toLocaleTimeString([], {
    hour: '2-digit',
    minute: '2-digit',
    hour12: true
  });
  isNew: boolean = true;

  constructor(private productsApiService: ProductsApiService,
    private fodmapLogService: FodmapLogService,
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router
  ) { }
    
  ngOnInit(): void {

    this.form = this.fb.group({
      id: 0,
      date: this.currDate,
      symptoms: this.fb.array([
      ]),
    })
    this.route.params.subscribe(params => {
      // check if this is existing record
      if (params['id']) {
        this.isNew = false;
        this.fodmapLogService.getSymptomsLogById(params['id']).subscribe(
          data => {
            this.form.patchValue(data);
            this.populateSymptoms(data.symptoms);
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

        console.log(this.products)
        console.log(this.symptomType[2])
    }
  populateSymptoms(symptoms: Symptom[]): void {
    const symptomsFormGroups = symptoms.map(symptoms => this.fb.group(symptoms));
    const symptomsFormArray = this.fb.array(symptomsFormGroups);
    this.form.setControl('symptoms', symptomsFormArray);
  }
   

  products: Product[];
  
  get symptomsArr(): FormArray {
    return this.form.get('symptoms') as FormArray;
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

  addSymptom(symptomType: SymptomType): void{
    this.symptomsArr.push(this.fb.group({
      symptomType: symptomType,
      symptomScale: 0
    }));
    // console.log(this.symptomsArr);
    this.quantityInput = 0;
    console.log(this.symptomType)
    
  }

  deleteSymptom(index: number): void{
    this.symptomsArr.removeAt(index);
  }

  saveSymptomsLog(): void{
    if(this.isNew){
    this.fodmapLogService.addSymptomsLog(this.form.value).subscribe(
      () => {},
      error => {
        console.log(error);
      },
      () => {
        this.router.navigate(['/daily-log', this.currDate]);
      }
    );
  } else {
    this.fodmapLogService.updateSymptomsLog(this.form.value).subscribe(
      () => {},
      error => {
        console.log(error);
      },
      () => {
        this.router.navigate(['/daily-log', this.currDate]);
      }
    );
  }
  }
  
  cancel(): void{
    this.form.reset();
  }

  onDateTimeChanged(newDate: Date) {
    this.form.controls['date'].setValue(newDate);
  }

}
