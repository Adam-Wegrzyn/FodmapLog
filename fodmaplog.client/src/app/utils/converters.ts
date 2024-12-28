export class Converters{

    static TotalKcalConverter(energyKcal100g: number, quantity: number, unit: string): number {
        return energyKcal100g * quantity  / 100;
}
}