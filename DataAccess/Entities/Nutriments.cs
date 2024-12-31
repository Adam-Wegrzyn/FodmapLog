using Newtonsoft.Json;

namespace DataAccess.Entities
{
    public class Nutriments : BaseEntity
    {
        public float? FatServing { get; set; }

        public float? Fat100g { get; set; }

        public string? FatUnit { get; set; }

        public float? SugarsServing { get; set; }

        public float? Sugars100g { get; set; }

        public string? SugarsUnit { get; set; }

        public float? CarbohydratesServing { get; set; }

        public float? Carbohydrates100g { get; set; }
        public string? CarbohydratesUnit { get; set; }
        public float? EnergyKcal100g { get; set; }

        public float? EnergyKcalServing { get; set; }
        public string? EnergyKcalUnit { get; set; }
        public float? ProteinsServing { get; set; }
        public float? Proteins100g { get; set; }
        public string? ProteinsUnit { get; set; }        
        public float? Fiber100g { get; set; }
        public float? FiberServing { get; set; }
        public string? FiberUnit { get; set; }

    }
}