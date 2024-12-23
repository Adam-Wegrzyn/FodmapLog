export class Nutriments{
    energyKcal100g: number;
    carbohydrates: number;
    carbohydratesServing: number;
    carbohydratesUnit: number;
    carbohydrates100g: number;
    fat: number;
    fatServing: number;
    fatUnit: number;
    fat100g: number;
    protein: number;
    proteinServing: number;
    proteinUnit: number;
    protein100g: number;
    fibre: number;
    servingQuantity: number;
    servingQuantityUnit: string;
    energyKcalServing: number;
    

    constructor(carbohydrates: number, protein: number, fat: number,
        fibre: number, carbohydratesServing: number, proteinServing: number,
        fatServing: number, carbohydrates100g: number, fat100g: number,
        protein100g: number, carbohydratesUnit: number, fatUnit: number,
        proteinUnit: number, energyKcal100g: number, servingQuantity: number,
        servingQuantityUnit: string, energyKcalServing: number){
        this.carbohydrates = carbohydrates;
        this.protein = protein;
        this.fat = fat;
        this.fibre = fibre;
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
    }
}