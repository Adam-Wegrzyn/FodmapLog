using DataAccess.Entities;
using DataAccess.Enums;

namespace Data.Common.DTO
{
    public class QuantityDto: BaseDto
    {
        public double Amount { get; set; }
        public Unit Unit { get; set; }
    }
}