using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Entities
{
    public class SymptomType: BaseEntity
    {
        public required string Name { get; set; }
    }
}
