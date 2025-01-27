using DataAccess.Entities;
using DataAccess.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Common.DTO
{
    public class SymptomsLogDto: BaseDto
    {
        public DateTime Date { get; set; }
        public List<SymptomDto> Symptoms { get; set; }
    }
}
