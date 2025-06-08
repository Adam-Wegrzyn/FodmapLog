using DataAccess.Enums;

namespace DataAccess.Entities
{
    public class ProductQuantity: BaseEntity
    {
        public required Product Product { get; set; }
        public float Quantity { get; set; }
        public required Unit Unit { get; set; }
    }
}