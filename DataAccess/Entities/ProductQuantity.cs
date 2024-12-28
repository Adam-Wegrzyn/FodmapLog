using DataAccess.Enums;

namespace DataAccess.Entities
{
    public class ProductQuantity: BaseEntity
    {
        public Product Product { get; set; }
        public float Quantity { get; set; }
        public Unit Unit { get; set; }
    }
}