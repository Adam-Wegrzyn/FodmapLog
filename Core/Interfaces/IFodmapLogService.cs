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
        Task<MealLog> GetMealLog(int id, CancellationToken cancellationToken);
        Task<IEnumerable<MealLog>> GetAllMealLogs(CancellationToken cancellationToken);
        Task<MealLog> AddMealLog(MealLog mealLog, CancellationToken cancellationToken);
        Task<MealLog> UpdateMealLog(MealLog mealLog, CancellationToken cancellationToken);
        Task<MealLog> DeleteMealLog(int id, CancellationToken cancellationToken);

        // Methods for Product
        Task<Product> GetProduct(int id, CancellationToken cancellationToken);
        Task<IEnumerable<Product>> GetAllProducts(CancellationToken cancellationToken);
        Task<Product> AddProduct(Product product, CancellationToken cancellationToken);
        Task<Product> UpdateProduct(Product product, CancellationToken cancellationToken);
        Task<Product> DeleteProduct(int id, CancellationToken cancellationToken);
    }
}
