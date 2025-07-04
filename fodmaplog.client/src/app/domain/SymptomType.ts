import { BaseClass } from "./BaseClass";

export class SymptomType 
extends BaseClass {
    name: string;

    constructor(id: number, name: string) {
        super(id);
        this.name = name;
    }
}