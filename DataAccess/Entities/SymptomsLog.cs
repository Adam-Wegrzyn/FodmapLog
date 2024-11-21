using DataAccess.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Entities
{
    public class SymptomsLog
    {
        public Symptom Symptom { get; set; }
        public SymptomScale SymptomScale { get; set; }
    }
}
