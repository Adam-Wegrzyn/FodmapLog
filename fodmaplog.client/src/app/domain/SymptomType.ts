import { BaseClass } from "./BaseClass";

/** 0 = problem (5 = serious). 1 = how I feel (5 = great). */
export type SymptomScaleKind = 0 | 1;
export const SCALE_KIND_NEGATIVE = 0 as const;
export const SCALE_KIND_POSITIVE = 1 as const;

export class SymptomType 
extends BaseClass {
    name: string;
    userId?: string | null;
    scaleKind?: SymptomScaleKind | number;

    constructor(id: number, name: string, userId?: string | null, scaleKind: SymptomScaleKind | number = 0) {
        super(id);
        this.name = name;
        this.userId = userId;
        this.scaleKind = scaleKind;
    }
}