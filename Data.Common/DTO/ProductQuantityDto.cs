using DataAccess.Entities;
using DataAccess.Enums;
using System.Text.Json.Serialization;

namespace Data.Common.DTO
{
    public class ProductQuantityDto: BaseDto
    {
        public ProductDto Product { get; set; }
        public double Quantity { get; set; }

        public Unit Unit { get; set; }
        //public double TotalGrams { get; set; }
    }
}