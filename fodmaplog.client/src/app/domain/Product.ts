import { Nutrients } from "./Nutrients";

export class Product {
    id: number;
    name: string;
    nutrients: Nutrients;

    constructor(id: number, name: string, nutrients: Nutrients){
        this.id = id;
        this.name = name;
        this.nutrients = nutrients;
    }
}
