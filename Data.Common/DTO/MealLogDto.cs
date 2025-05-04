using DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Common.DTO
{
    public class MealLogDto: BaseDto
    {
        public DateTime Date { get; set; }
        public IEnumerable<ProductQuantityDto>? ProductQuantity { get; set; }
        public float? TotalKcal { get; set; }  
        public float? TotalCarbohydrates { get; set; }  
        public float? TotalProteins { get; set; }  
        public float? TotalFat { get; set; }  
        public float? TotalFiber { get; set; }
    }
}
