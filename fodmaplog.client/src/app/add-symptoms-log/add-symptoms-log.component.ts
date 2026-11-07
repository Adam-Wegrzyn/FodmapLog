import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { Product } from '../domain/Product';
import { ProductsApiService } from '../services/products-api-service';
import { FormArray, FormBuilder, FormGroup } from '@angular/forms';
import { Unit } from '../domain/Unit';
import { FodmapLogService } from '../services/fodmap-log-service';
import { Symptom } from '../domain/Symptom';
import { SymptomsLog } from '../domain/SymptomsLog';
import {
  SCALE_KIND_NEGATIVE,
  SCALE_KIND_POSITIVE,
  SymptomScaleKind,
  SymptomType
} from '../domain/SymptomType';
import { SymptomScale } from '../domain/SymptomScale';
import { ActivatedRoute, Router } from '@angular/router';
import { SymptomTypesService } from '../services/symptom-types.service';
import { SymptomsLogTransferService } from '../services/symptoms-log-transfer.service';
import { isPositiveScale, resolveSymptomTypeId, scaleTone } from '../services/reference-i18n';
import { combineLocalDateTime, formatClockTime, formatDateInput } from '../services/clock-time';
import { TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-add-symptoms-log',
  templateUrl: './add-symptoms-log.component.html',
  styleUrl: './add-symptoms-log.component.css'
})
export class addSymptomsLogComponent implements OnInit {

  @ViewChild('closeModalBtn') closeModalBtn: ElementRef;
  form: FormGroup;
  name: string;
  customName = '';
  customScaleKind: SymptomScaleKind = SCALE_KIND_NEGATIVE;
  symptomTypes: SymptomType[] = [];
  selectedUnit: string = "1";
  quantityInput: number = 1;
  symptomsLog: SymptomsLog;
  symptomType: SymptomType;
  symptomRange: number;
  symptomScale = SymptomScale;
  currDate = formatDateInput();
  currTime = formatClockTime();
  isNew: boolean = true;
  saveError: string | null = null;
  isSaving = false;
  customError: string | null = null;
  isAddingCustom = false;

  constructor(private productsApiService: ProductsApiService,
    private fodmapLogService: FodmapLogService,
    private symptomTypeService: SymptomTypesService,
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private SymptomsLogTransferService: SymptomsLogTransferService,
    private translate: TranslateService
  ) { }

  symptomKey(type: { id?: number; name?: string } | null | undefined): string {
    const id = resolveSymptomTypeId(type);
    if (id != null) {
      return `ref.symptom.${id}`;
    }
    return type?.name || '';
  }

  isPersonal(type: SymptomType | null | undefined): boolean {
    return !!type?.userId;
  }

  isWellbeing(type: SymptomType | null | undefined): boolean {
    return isPositiveScale(type);
  }

  setCustomScaleKind(kind: SymptomScaleKind): void {
    this.customScaleKind = kind === SCALE_KIND_POSITIVE
      ? SCALE_KIND_POSITIVE
      : SCALE_KIND_NEGATIVE;
  }

  ngOnInit(): void {

    const qDate = this.route.snapshot.queryParamMap.get('date');
    if (qDate) {
      this.currDate = qDate;
    }

    this.form = this.fb.group({
      id: 0,
      date: combineLocalDateTime(this.currDate, this.currTime),
      symptoms: this.fb.array([
      ]),
    });
    this.route.params.subscribe(params => {
      // check if this is existing record
      if (params['id']) {
        this.isNew = false;
        this.fodmapLogService.getSymptomsLogById(params['id']).subscribe(
          data => {
            this.form.patchValue(data);
            this.populateSymptoms(data.symptoms);
            const when = new Date(data.date);
            this.currDate = formatDateInput(when);
            this.currTime = formatClockTime(when);
            this.applyWhenToForm();
          },
          () => {},
          () => {
            console.log(this.form.value)
          }
        )
      }
        else if (this.route.snapshot.queryParamMap.get('isPending') == 'true') {
        var pendingSymptomsLog = this.SymptomsLogTransferService.symptomsLog;
        if (pendingSymptomsLog){
                      const when = new Date(pendingSymptomsLog.date);
                      this.currTime = formatClockTime(when);
          pendingSymptomsLog.date = combineLocalDateTime(this.currDate, this.currTime);
          this.form.patchValue(pendingSymptomsLog);
          this.populateSymptoms(pendingSymptomsLog.symptoms);
        }
      }
    });

    //fill symptom types
    this.symptomTypeService.getSymptomTypes().subscribe(
      (result: SymptomType[]) => this.symptomTypes = result,
      (error) => console.log(error),  
    )
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
    });
  }

  addSymptom(symptomType: SymptomType): void{
    if (this.isAdded(symptomType)) {
      return;
    }
    this.symptomsArr.push(this.fb.group({
      symptomType: symptomType,
      symptomScale: 2
    }));
  }

  addCustomSymptom(): void {
    const name = (this.customName || '').trim();
    if (!name || this.isAddingCustom) {
      return;
    }

    const existing = this.symptomTypes.find(
      t => (t.name || '').trim().toLowerCase() === name.toLowerCase()
    );
    if (existing) {
      this.addSymptom(existing);
      this.customName = '';
      this.customError = null;
      return;
    }

    this.isAddingCustom = true;
    this.customError = null;
    this.symptomTypeService.addSymptomType(name, this.customScaleKind).subscribe({
      next: (created) => {
        this.isAddingCustom = false;
        this.symptomTypes = [created, ...this.symptomTypes.filter(t => t.id !== created.id)];
        this.addSymptom(created);
        this.customName = '';
        this.customScaleKind = SCALE_KIND_NEGATIVE;
      },
      error: () => {
        this.isAddingCustom = false;
        this.customError = this.translate.instant('symptoms.ownFail');
      }
    });
  }

  isAdded(symptomType: SymptomType): boolean {
    return (this.symptomsArr?.controls || []).some(
      c => c.value?.symptomType?.id === symptomType.id
        || c.value?.symptomType?.name === symptomType.name
    );
  }

  toneFor(scale: number, type?: SymptomType | null): 'calm' | 'mild' | 'hot' {
    return scaleTone(scale, type);
  }

  deleteSymptom(index: number): void{
    this.symptomsArr.removeAt(index);
  }

  saveSymptomsLog(): void {
    this.saveError = null;
    this.applyWhenToForm();
    this.isSaving = true;
    const request = this.isNew
      ? this.fodmapLogService.addSymptomsLog(this.form.value)
      : this.fodmapLogService.updateSymptomsLog(this.form.value);
    request.subscribe({
      next: () => {
        this.isSaving = false;
        this.router.navigate(['/daily-log', this.currDate]);
      },
      error: () => {
        this.isSaving = false;
        this.saveError = this.translate.instant('symptoms.saveFail');
      }
    });
  }
  
  cancel(): void{
    this.form.reset();
  }

  onDateTimeChanged(when: string) {
    this.form.controls['date'].setValue(when);
  }

  private applyWhenToForm(): void {
    this.form.controls['date'].setValue(combineLocalDateTime(this.currDate, this.currTime));
  }

}
