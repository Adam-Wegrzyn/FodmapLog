export class Converters{

    static TotalKcalConverter(energyKcal100g: number, totalGrams: number): number {
        return energyKcal100g * totalGrams / 100;
}
}