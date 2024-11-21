using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Entities
{
    public class MealLog
    {
        public List<Meal> Meal { get; set; }
        public DateTime Time { get; set; }
    }
}
