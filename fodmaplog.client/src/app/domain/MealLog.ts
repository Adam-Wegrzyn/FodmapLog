import { Product } from "./Product";
import { ProductQuantity } from "./ProductQuantity";

export class MealLog{
    id: number;
    date: string;
    productQuantity: ProductQuantity[];

    constructor(id: number, date: string, productQuantity: ProductQuantity[],){
        this.id = id;
        this.date = date;
        this.productQuantity = productQuantity;
    }
}