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
        /// <summary>
        /// Null = shared catalog (seeded). Set = this user's private type.
        /// </summary>
        public string? UserId { get; set; }
    }
}
