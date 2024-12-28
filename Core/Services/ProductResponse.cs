using Data.Common.DTO;
using DataAccess.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Services
{
    public class ProductResponse
    {
        //public Product Product { get; set; }

        //public string Code { get; set; }

        //public bool? Status { get; set; }

        //[JsonProperty("status_verbose")]
        //public string StatusVerbose { get; set; }

        public int Count { get; set; }
        public int Page { get; set; }
        public int PageCount { get; set; }
        public int PageSize { get; set; }


        public IEnumerable<ProductDto> Products { get; set; }

        //public string Code { get; set; }

        //public bool? Status { get; set; }

        //[JsonProperty("status_verbose")]
        //public string StatusVerbose { get; set; }
    }
}
