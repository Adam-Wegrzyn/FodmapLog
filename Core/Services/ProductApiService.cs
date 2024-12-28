using AutoMapper;
using Core.Interfaces;
using Data.Common.DTO;
using DataAccess;
using DataAccess.Entities;
using DataAccess.Interfaces;
using DataAccess.Repositories;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web.Http;

namespace Core.Services
{

    
    public class ProductApiService : IProductsApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IFodmapLogRepository _fodmapLogRepository;
        private readonly IMapper _mapper;

        public ProductApiService(HttpClient httpClient, IFodmapLogRepository fodmapLogRepository, IMapper mapper)
        {
            _httpClient = httpClient;
            _fodmapLogRepository = fodmapLogRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ProductDto>> GetProductsByName(string name)
        {
            var url = $"https://world.openfoodfacts.org/cgi/search.pl?search_terms={name}&search_simple=1&json=1";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var jsonString = await response.Content.ReadAsStringAsync();
            var productsResponse = JsonConvert.DeserializeObject<ProductResponse>(jsonString);
            var products = productsResponse.Products;

            foreach (var product in products)
            {
                
                var p = await _fodmapLogRepository.GetProductByExternalId(product.IdExternal);
                if (p == null && product.IdExternal != null)
                {
                    var productToAdd = _mapper.Map<Product>(product);
                    await _fodmapLogRepository.AddProduct(productToAdd);
                }
            }



            return products;
        }
    }
}

