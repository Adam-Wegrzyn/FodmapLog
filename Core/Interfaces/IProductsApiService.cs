using Core.Services;
using Data.Common.DTO;
using DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IProductsApiService
    {

        Task<IEnumerable<ProductDto>> GetProductsByName(string name, CancellationToken cancellationToken);
    }
}
