using DataAccess.Entities;
using Newtonsoft.Json;

namespace Data.Common.DTO
{
    public class ProductDto: BaseDto
    {
        [JsonProperty("_id")]
        public string IdExternal { get; set; }

        [JsonProperty("product_name")]
        public string Name { get; set; }

        [JsonProperty("product_quantity")]
        public string ProductQuantity { get; set; }

        [JsonProperty("product_quantity_unit")]
        public string ProductQuantityUnit { get; set; }

        [JsonProperty("serving_quantity")]
        public string ServingQuantity { get; set; }
        [JsonProperty("serving_quantity_unit")]
        public string ServingQuantityUnit { get; set; }
        public NutrimentsDto Nutriments { get; set; }
    }
}