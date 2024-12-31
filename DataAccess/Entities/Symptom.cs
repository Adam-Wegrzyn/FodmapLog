using DataAccess.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Entities
{
    public class Symptom: BaseEntity
    {
        public SymptomType SymptomType { get; set; }
        public SymptomScale SymptomScale { get; set; }
    }
}
