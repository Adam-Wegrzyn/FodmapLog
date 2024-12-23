import { Nutriments } from "./Nutriments";

export class Product {
    id: number;
    name: string;
    productQuantity: string;
    productQuantityUnit: string;
    servingQuantity: number;
    servingQuantityUnit: string;
    nutriments: Nutriments;

    constructor(id: number, name: string, nutriments: Nutriments,
        productQuantity: string, productQuantityUnit: string, servingQuantity: number, servingQuantityUnit: string) {
        this.id = id;
        this.name = name;
        this.nutriments = nutriments;
        this.productQuantity = productQuantity;
        this.productQuantityUnit = productQuantityUnit;
        this.servingQuantity = servingQuantity;
        this.servingQuantityUnit = servingQuantityUnit;
    }
}
