using Data.Common.DTO;
using DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IFodmapLogService
    {
        Task<MealDto> GetMeal(int id, CancellationToken cancellationToken);
        Task<IEnumerable<MealDto>> GetAllMeals(CancellationToken cancellationToken);
        Task<MealDto> AddMeal(MealDto mealDto, CancellationToken cancellationToken);
        Task<MealDto> UpdateMeal(MealDto mealDto, CancellationToken cancellationToken);
        Task<MealDto> DeleteMeal(int id, CancellationToken cancellationToken);

        // Methods for Product
        Task<ProductDto> GetProduct(int id, CancellationToken cancellationToken);
        Task<IEnumerable<ProductDto>> GetAllProducts(CancellationToken cancellationToken);
        Task<ProductDto> AddProduct(ProductDto productDto, CancellationToken cancellationToken);
        Task<ProductDto> UpdateProduct(ProductDto productDto, CancellationToken cancellationToken);
        Task<ProductDto> DeleteProduct(int id, CancellationToken cancellationToken);

    }
}
