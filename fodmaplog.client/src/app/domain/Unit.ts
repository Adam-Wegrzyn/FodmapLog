import { BaseClass } from "./BaseClass";

export class Unit extends BaseClass {
    name: string;

    constructor(id: number, name: string) {
        super(id);
        this.name = name;
    }
}