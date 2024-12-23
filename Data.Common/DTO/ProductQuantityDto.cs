using DataAccess.Enums;

namespace Data.Common.DTO
{
    public class ProductQuantityDto: BaseDto
    {
        public ProductDto Product { get; set; }
        public double Quantity { get; set; }
        public Unit Unit { get; set; }
        public double TotalGrams { get; set; }
    }
}