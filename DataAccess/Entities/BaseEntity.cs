using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace DataAccess.Entities
{
    public class BaseEntity
    {
        [JsonIgnore]
        public int Id { get; set; }
    }
}
