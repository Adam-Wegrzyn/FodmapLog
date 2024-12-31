using Core.Interfaces;
using DataAccess.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.Tracing;

namespace FodmapLog.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsApiController : ControllerBase
    {
        private readonly IProductsApiService _productsApiService;

        public ProductsApiController(IProductsApiService productsApiService)
        {
            _productsApiService = productsApiService;
        }

        [HttpGet]
        [Route("getProductsByKeyword/{keyword}")]
        public async Task<IActionResult> GetProductsByKeyword(string keyword, CancellationToken cancellationToken)
        {
            var result = await _productsApiService.GetProductsByName(keyword, cancellationToken);
            return Ok(result);

        }
    }
}
