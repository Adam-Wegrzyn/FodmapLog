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

    }
}
