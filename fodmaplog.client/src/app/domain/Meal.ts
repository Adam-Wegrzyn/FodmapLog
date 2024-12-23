import { Nutriments } from "./Nutriments";
import { Product } from "./Product";
import { ProductQuantity } from "./ProductQuantity";

export class Meal{
    id: number;
    date: string;
    productQuantity: ProductQuantity[];
    totalNutriments: Nutriments;
    totalKcal: number;

    constructor(id: number, date: string, productQuantity: ProductQuantity[],
        totalNutriments: Nutriments, totalKcal: number){
        this.id = id;
        this.date = date;
        this.productQuantity = productQuantity;
        this.totalNutriments = totalNutriments;
        this.totalKcal = totalKcal;
    }
}