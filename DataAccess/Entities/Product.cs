using Newtonsoft.Json;

namespace DataAccess.Entities
{
    public class Product
    {
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
        public Nutriments Nutriments { get; set; }
    }
}