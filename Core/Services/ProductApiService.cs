using Core.Interfaces;
using DataAccess.Entities;
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

        public ProductApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<Product>> GetProductsByName(string name)
        {
            var url = $"https://world.openfoodfacts.org/cgi/search.pl?search_terms={name}&search_simple=1&json=1";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var jsonString = await response.Content.ReadAsStringAsync();
            var productsResponse = JsonConvert.DeserializeObject<ProductResponse>(jsonString);
            var products = productsResponse.Products;
           // var product = productsResponse.Product;


            return products;
        }
    }
}

