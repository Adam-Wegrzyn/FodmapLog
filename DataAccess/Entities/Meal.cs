using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Entities
{
    public class Meal
    {
        Product Product { get; set; }
        Quantity Quantity { get; set; }
    }
}
