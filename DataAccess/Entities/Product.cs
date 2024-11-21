using Newtonsoft.Json;

namespace DataAccess.Entities
{
    public class Product
    {
        [JsonProperty("product_name")]
        public string Name;
        public Nutrients nutrients;
    }
}