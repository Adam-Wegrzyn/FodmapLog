using DataAccess.Entities;
using Newtonsoft.Json;

namespace Data.Common.DTO
{
    public class ProductDto: BaseDto
    {
        [JsonProperty("product_name")]
        public string Name { get; set; }

        //[JsonProperty("product_quantity")]
        //public string ProductQuantity { get; set; }

        //[JsonProperty("product_quantity_unit")]
        //public string ProductQuantityUnit { get; set; }


    }
}