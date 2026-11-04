import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { Product } from '../domain/Product';
import { ProductsApiService } from '../services/products-api-service';
import { FormArray, FormBuilder, FormControl, FormGroup } from '@angular/forms';
import { Unit } from '../domain/Unit';
import { getLocaleDateTimeFormat } from '@angular/common';
import { MealLog } from '../domain/MealLog';
import { FodmapLogService } from '../services/fodmap-log-service';
import { ActivatedRoute, Router } from '@angular/router';
import { combineLocalDateTime, formatClockTime, formatDateInput } from '../services/clock-time';
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
  form: FormGroup;
  name: string;
  units: Unit[];
  selectedUnit: Unit;
  quantityInput: number = 1;
  mealLog: MealLog;
  unit: Unit;
  isNew: boolean = true;
  currDate = formatDateInput();
  currTime = formatClockTime();
  isPending: boolean = false;
  showComposer = false;
  saveError: string | null = null;
  isSaving = false;

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
      date: combineLocalDateTime(this.currDate, this.currTime),
      productQuantity: this.fb.array([
      ]),
    });
    this.route.params.subscribe(params => {
      // check if this is existing record
      if (params['id']) {
        this.isNew = false;
        this.fodmapLogService.getMealLogById(params['id']).subscribe(
          data => {
            this.form.patchValue(data);
            this.setProductQuantities(data.productQuantity);
            const when = new Date(data.date);
            this.currDate = formatDateInput(when);
            this.currTime = formatClockTime(when);
            this.applyWhenToForm();
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
            const when = new Date(pendingMealLog.date);
            this.currTime = formatClockTime(when);
            pendingMealLog.date = combineLocalDateTime(this.currDate, this.currTime);
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
    this.saveError = null;
    this.applyWhenToForm();
    this.isSaving = true;
    const request = this.isNew
      ? this.fodmapLogService.addMealLog(this.form.value)
      : this.fodmapLogService.updateMealLog(this.form.value);
    request.subscribe({
      next: () => {
        this.isSaving = false;
        this.router.navigate(['/daily-log', this.currDate]);
      },
      error: () => {
        this.isSaving = false;
        this.saveError = this.translate.instant('meal.saveFail');
      }
    });
  }


  cancel(): void {
    this.form.reset();
  }

  onDateTimeChanged(when: string) {
    this.form.controls['date'].setValue(when);
  }

  private applyWhenToForm(): void {
    this.form.controls['date'].setValue(combineLocalDateTime(this.currDate, this.currTime));
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


