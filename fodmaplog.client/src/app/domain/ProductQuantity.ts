import { BaseClass } from "./BaseClass";
import { Product } from "./Product";
import { Unit } from "./Unit";
import { Converters } from "../utils/converters";

export class ProductQuantity extends BaseClass {
    product: Product;
    quantity: number;
    unit: Unit;
    totalGrams: number;
    totalKcal: number;

    constructor(id: number, product: Product, quantity: number, unit: Unit, totalKcal: number) {
        super(id);
        this.product = product;
        this.quantity = quantity;
        this.unit = unit;
        this.totalGrams = this.quantity * this.unit;
        this.totalKcal = Converters.TotalKcalConverter(this.product.nutriments.energyKcal100g, this.totalGrams);
    }
}

}
