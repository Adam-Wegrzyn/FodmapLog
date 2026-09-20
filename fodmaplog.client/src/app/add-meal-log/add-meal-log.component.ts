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
import { MealLogTransferService } from '../services/meal-log-transfer.service';
import { SymptomsLogTransferService } from '../services/symptoms-log-transfer.service';
import { Symptom } from '../domain/Symptom';
import { UnitService } from '../services/unit.service';
import { TranslateService } from '@ngx-translate/core';
import { resolveUnitId, translateUnit } from '../services/reference-i18n';


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
  units: Unit[];
  selectedUnit: Unit;
  quantityInput: number = 1;
  mealLog: MealLog;
  unit: Unit;
  isNew: boolean = true;
  currDate = new Date().toISOString().split('T')[0];
  currTime = new Date().toLocaleTimeString([], {
    hour: '2-digit',
    minute: '2-digit',
    hour12: true
  });
  isPending: boolean = false;
  showComposer = false;

  constructor(private productsApiService: ProductsApiService,
    private fodmapLogService: FodmapLogService,
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private mealLogTransferService: MealLogTransferService,
    private unitService: UnitService,
    private translate: TranslateService,
  ) { }

  unitLabel(unit: Unit | null | undefined): string {
    return translateUnit(this.translate, unit);
  }

  unitKey(unit: Unit | null | undefined): string {
    const id = resolveUnitId(unit);
    return id != null ? `ref.unit.${id}` : (unit?.name || '');
  }

  ngOnInit(): void {
    this.fillUnits();
    const qDate = this.route.snapshot.queryParamMap.get('date');
    if (qDate) {
      this.currDate = qDate;
    }
    this.form = this.fb.group({
      id: 0,
      date: new Date(),
      productQuantity: this.fb.array([
      ]),
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
          () => { },
          () => {
            console.log(this.form.value)
          }
        )
      }
      else if (this.route.snapshot.queryParamMap.get('isPending') == 'true') {
        var pendingMealLog = this.mealLogTransferService.mealLog;
        if (pendingMealLog) {
            this.currTime = new Date(pendingMealLog.date).toLocaleTimeString([], {
            hour: '2-digit',
            minute: '2-digit',
            hour12: false
          });
          pendingMealLog.date = this.currDate + 'T' + this.currTime;
          pendingMealLog
          this.form.patchValue(pendingMealLog);

          this.setProductQuantities(pendingMealLog.productQuantity);
        }
      }
    });
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


  AddProduct(productName: string, quantity: number): void {
    if (!productName?.trim() || !this.selectedUnit) {
      return;
    }
    const product: Product = {
      id: 0,
      name: productName.trim(),
    };
    this.productQuantityArr.push(this.fb.group({
      product: product,
      quantity: quantity,
      unit: this.selectedUnit,
    }));
    this.quantityInput = 1;
    this.name = '';
    this.closeComposer();
  }

  openComposer(): void {
    if (!this.name?.trim()) {
      return;
    }
    if (!this.selectedUnit && this.units?.length) {
      this.selectedUnit = this.units[0];
    }
    this.quantityInput = this.quantityInput || 1;
    this.showComposer = true;
  }

  closeComposer(): void {
    this.showComposer = false;
  }

  confirmAddProduct(): void {
    this.AddProduct(this.name, this.quantityInput);
  }

  DeleteProduct(index: number): void {
    this.productQuantityArr.removeAt(index);
  }

  SaveMealLog(): void {
    console.log('SaveMealLog');
    console.log(this.mealLog)
    console.log(this.form.value);
    if (this.isNew) {
      this.fodmapLogService.addMealLog(this.form.value).subscribe(
        () => { },
        () => { },

        () => {
          this.router.navigate(['/daily-log', this.currDate]);
        }
      );
    }
    else {
      this.fodmapLogService.updateMealLog(this.form.value).subscribe(
        () => { },
        () => { },
        () => {
          this.router.navigate(['/daily-log', this.currDate]);
        }
      );
    }
  }


  cancel(): void {
    this.form.reset();
  }

  onDateTimeChanged(newDate: Date) {
    this.form.controls['date'].setValue(newDate);
  }

  fillUnits(): void {
    this.unitService.getAllUnits().subscribe(
      (data) => {
        this.units = data;
        if (!this.selectedUnit && data?.length) {
          this.selectedUnit = data[0];
        }
      },
      () => { console.log('Error fetching units'); },
    );
  }

}


