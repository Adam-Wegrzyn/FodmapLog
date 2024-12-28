using Newtonsoft.Json;

namespace DataAccess.Entities
{
    public class Nutriments : BaseEntity
    {
        public float? Fat { get; set; }

        public float? FatServing { get; set; }

        public float? Fat100g { get; set; }

        public string? FatUnit { get; set; }

        public float? Sugars { get; set; }

        public float? SugarsServing { get; set; }

        public float? Sugars100g { get; set; }

        public string? SugarsUnit { get; set; }

        public float? Carbohydrates { get; set; }

        public float? Carbohydrates100g { get; set; }
        public string? CarbohydratesUnit { get; set; }
        public float? EnergyKcal100g { get; set; }

        public float? EnergyKcalServing { get; set; }
        public string? EnergyKcalUnit { get; set; }
        public float? Proteins { get; set; }
        public float? ProteinsServing { get; set; }
        public float? Proteins100g { get; set; }
        public string? ProteinsUnit { get; set; }

    }
}