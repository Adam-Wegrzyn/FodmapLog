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
        public Task<IEnumerable<Meal>> GetAllMeals();
        public Task<Meal> GetMeal(int id);
        public Task<Meal> AddMeal(Meal Meal);
        public Task<Meal> UpdateMeal(Meal fodmapLog);
        public Task<Meal> DeleteMeal(int id);
        public Task<IEnumerable<Product>> GetAllProducts();
        public Task<Product> GetProduct(int id);
        public Task<Product> AddProduct(Product product);
        public Task<Product> UpdateProduct(Product product);
        public Task<Product> DeleteProduct(int id);

    }
}
