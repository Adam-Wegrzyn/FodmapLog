import { DailyLog } from "./DailyLog";

export interface DailyLogUI extends DailyLog {
  isPending?: boolean;
  isEditing?: boolean;
}