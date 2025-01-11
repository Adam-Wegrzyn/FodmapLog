import { Symptom } from "./Symptom";

export class SymptomsLog {
    id: number;
    date: string;
    symptoms: Symptom[];

    constructor(id: number, date: string, symptoms: Symptom[]) {
        this.id = id;
        this.date = date;
        this.symptoms = symptoms;   
    }
}