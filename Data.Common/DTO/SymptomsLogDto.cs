using DataAccess.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Common.DTO
{
    public class SymptomsLogDto
    {
        public SymptomType Symptom { get; set; }
        public SymptomScale SymptomScale { get; set; }
    }
}
