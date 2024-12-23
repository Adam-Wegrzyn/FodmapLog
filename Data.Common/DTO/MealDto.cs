using DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Common.DTO
{
    public class MealDto: BaseDto
    {
        public DateTime Date { get; set; }
        public IEnumerable<ProductQuantityDto>? ProductQuantity { get; set; }
        public double TotalKcal { get; set; }  
        public double TotalCarbohydrates { get; set; }  
        public double TotalProteins { get; set; }  
        public double TotalFat { get; set; }  
    }
}
