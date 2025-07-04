import { Injectable } from '@angular/core';
import { SymptomsLog } from '../domain/SymptomsLog';

@Injectable({
  providedIn: 'root'
})
export class SymptomsLogTransferService {
  symptomsLog: SymptomsLog | null = null;
  constructor() { }
}
