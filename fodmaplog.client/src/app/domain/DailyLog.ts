import { BaseClass } from "./BaseClass";
import { MealLog } from "./MealLog";
import { SymptomsLog } from "./SymptomsLog";


export class DailyLog extends BaseClass {
    date: string;
    mealLogs: MealLog[];
    symptomsLog: SymptomsLog[];
    constructor(id: number, date: string, mealLogs: MealLog[]) {
        super(id);
        this.date = date;
        this.mealLogs = mealLogs;
    }
}
