using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Entities
{
    public class MealLog: BaseEntity
    {
        public DateTime Date { get; set; }
        public IEnumerable<ProductQuantity> ProductQuantity { get; set; }
        public float? TotalKcal { get; set; } = 0;
        public float? TotalCarbohydrates { get; set; }
        public float? TotalProteins { get; set; }
        public float? TotalFat { get; set; }
        public float? TotalFibre { get;}
    }
}
