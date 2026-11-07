import { BaseClass } from "./BaseClass";

export class SymptomType 
extends BaseClass {
    name: string;
    userId?: string | null;

    constructor(id: number, name: string, userId?: string | null) {
        super(id);
        this.name = name;
        this.userId = userId;
    }
}