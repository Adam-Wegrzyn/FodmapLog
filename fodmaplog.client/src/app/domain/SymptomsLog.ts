import { SymptomScale } from "./SymptomScale";
import { SymptomType } from "./SymptomType";

export class SymptomsLog {
    id: number;
    date: string;
    symptomType: SymptomType;
    symptomScale: SymptomScale;

    constructor(id: number, date: string, symptomType: SymptomType,
         symptomScale: SymptomScale) {
        this.id = id;
        this.date = date;
        this.symptomType = symptomType;
        this.symptomScale = symptomScale;
        
        
    }
}