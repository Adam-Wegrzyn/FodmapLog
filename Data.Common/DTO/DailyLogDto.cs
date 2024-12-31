using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Common.DTO
{
    public class DailyLogDto: BaseDto
    {
        public DateTime Date { get; set; }
        public MealLogDto? MealLog { get; set; }
        public SymptomsLogDto? SymptomsLog { get; set; }

    }
}
