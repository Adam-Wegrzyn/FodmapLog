using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Common.DTO
{
    public class NutrimentsDto
    {
        [JsonProperty("fat")]
        public float? Fat { get; set; }

        [JsonProperty("fat_serving")]
        public float? FatServing { get; set; }

        [JsonProperty("fat_100g")]
        public float? Fat100g { get; set; }

        [JsonProperty("fat_unit")]
        public string FatUnit { get; set; }

        [JsonProperty("sugars")]
        public float? Sugars { get; set; }

        [JsonProperty("sugars_serving")]
        public float? SugarsServing { get; set; }

        [JsonProperty("sugars_100g")]
        public float? Sugars100g { get; set; }

        [JsonProperty("sugars_unit")]
        public string SugarsUnit { get; set; }

        [JsonProperty("carbohydrates")]
        public float? Carbohydrates { get; set; }

        [JsonProperty("carbohydrates_100g")]
        public float? Carbohydrates100g { get; set; }

        [JsonProperty("carbohydrates_unit")]
        public string? CarbohydratesUnit { get; set; }

        [JsonProperty("energy-kcal_100g")]
        public float? EnergyKcal100g { get; set; }

        [JsonProperty("energy-kcal_serving")]
        public float? EnergyKcalServing { get; set; }

        [JsonProperty("energy-kcal_unit")]
        public string EnergyKcalUnit { get; set; }

        [JsonProperty("proteins")]
        public float? Proteins { get; set; }

        [JsonProperty("proteins_serving")]
        public float? ProteinsServing { get; set; }

        [JsonProperty("proteins_100g")]
        public float? Proteins100g { get; set; }

        [JsonProperty("proteins_unit")]
        public string ProteinsUnit { get; set; }
    }
}
