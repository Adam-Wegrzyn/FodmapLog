using DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Common.DTO
{
    public class MealDto
    {
        public Product Product { get; set; }
        public Quantity Quantity { get; set; }
    }
}
