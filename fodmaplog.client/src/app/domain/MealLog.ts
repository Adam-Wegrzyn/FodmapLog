import { Nutriments } from "./Nutriments";
import { Product } from "./Product";
import { ProductQuantity } from "./ProductQuantity";

export class MealLog{
    id: number;
    date: string;
    productQuantity: ProductQuantity[];
    totalNutriments: Nutriments;
    totalKcal: number;
    totalCarbohydrates: number;
    totalProteins: number;
    totalFats: number;

    constructor(id: number, date: string, productQuantity: ProductQuantity[],
        totalNutriments: Nutriments, totalKcal: number,
        totalCarbohydrates: number, totalProteins: number, totalFats: number){
        this.id = id;
        this.date = date;
        this.productQuantity = productQuantity;
        this.totalNutriments = totalNutriments;
        this.totalKcal = totalKcal;
        this.totalCarbohydrates = totalCarbohydrates;
        this.totalProteins = totalProteins;
        this.totalFats = totalFats;
    }
}