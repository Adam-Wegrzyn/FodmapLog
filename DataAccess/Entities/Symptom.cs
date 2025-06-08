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
        public required SymptomType SymptomType { get; set; }
        public required SymptomScale SymptomScale { get; set; }
    }
}
