using DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Interfaces
{
    public interface IFodmapLogRepository
    {
        public Task<IEnumerable<MealLog>> GetAllMealLogs();
        public Task<MealLog> GetMealLog(int id);
        public Task<MealLog> AddMealLog(MealLog mealLog);
        public Task<MealLog> UpdateMealLog(MealLog fodmapLog);
        public Task<MealLog> DeleteMealLog(int id);
        public Task<IEnumerable<Product>> GetAllProducts();
        public Task<Product> GetProduct(int id);
        public Task<Product> AddProduct(Product product);
        public Task<Product> UpdateProduct(Product product);
        public Task<Product> DeleteProduct(int id);

    }
}
