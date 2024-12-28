using Newtonsoft.Json;

namespace DataAccess.Entities
{
    public class Product: BaseEntity
    {
        public string IdExternal { get; set; }

        public string? Name { get; set; }

        public string? ProductQuantity { get; set; }        
        
        public string? ProductQuantityUnit { get; set; }

        public string? ServingQuantity { get; set; }

        public string? ServingQuantityUnit { get; set; }
        public Nutriments? Nutriments { get; set; }
    }
}