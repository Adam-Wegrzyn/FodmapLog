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
    }
}
