import { SymptomScale } from "./SymptomScale";
import { SymptomType } from "./SymptomType";

export class Symptom {
    id: number;
    symptomType: SymptomType;
    symptomScale: SymptomScale;
    constructor(id: number, symptomType: SymptomType, symptomScale: SymptomScale) {
        this.id = id;
        this.symptomType = symptomType;
        this.symptomScale = symptomScale;
    }
}