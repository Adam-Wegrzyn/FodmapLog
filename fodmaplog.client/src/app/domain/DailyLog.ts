import { BaseClass } from "./BaseClass";
import { MealLog } from "./MealLog";
import { SymptomsLog } from "./SymptomsLog";


export class DailyLog extends BaseClass {
    date: string;
    mealLog: MealLog;
    symptomsLog: SymptomsLog;
    constructor(id: number, date: string, mealLog: MealLog, symptomsLog: SymptomsLog) {
        super(id);
        this.date = date;
        this.mealLog = mealLog;
        this.symptomsLog = symptomsLog;
    }
}
