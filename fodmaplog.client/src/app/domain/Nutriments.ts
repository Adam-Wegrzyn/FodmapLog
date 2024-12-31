export class Nutriments {
    energyKcal100g: number;
    carbohydratesServing: number;
    carbohydratesUnit: number;
    carbohydrates100g: number;
    fatServing: number;
    fatUnit: number;
    fat100g: number;
    proteinServing: number;
    proteinUnit: number;
    protein100g: number;
    servingQuantity: number;
    servingQuantityUnit: string;
    energyKcalServing: number;
    fiber100g: number;
    fiberServing: number;
    fiberUnit: string;

    constructor(
        carbohydratesServing: number,
        proteinServing: number,
        fatServing: number,
        carbohydrates100g: number,
        fat100g: number,
        protein100g: number,
        carbohydratesUnit: number,
        fatUnit: number,
        proteinUnit: number,
        energyKcal100g: number,
        servingQuantity: number,
        servingQuantityUnit: string,
        energyKcalServing: number,
        fiber100g: number,
        fiberServing: number,
        fiberUnit: string

    ) {
        this.carbohydratesServing = carbohydratesServing;
        this.proteinServing = proteinServing;
        this.fatServing = fatServing;
        this.carbohydrates100g = carbohydrates100g;
        this.fat100g = fat100g;
        this.protein100g = protein100g;
        this.carbohydratesUnit = carbohydratesUnit;
        this.fatUnit = fatUnit;
        this.proteinUnit = proteinUnit;
        this.energyKcal100g = energyKcal100g;
        this.servingQuantity = servingQuantity;
        this.servingQuantityUnit = servingQuantityUnit;
        this.energyKcalServing = energyKcalServing;
        this.fiber100g = this.fiber100g
        this.fiberServing = this.fiberServing
        this.fiberUnit = this.fiberUnit
    }
}