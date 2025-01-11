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
        public Task<IEnumerable<MealLog>> GetAllMealLogs(CancellationToken cancellationToken);
        public Task<MealLog> GetMealLogById(int id, CancellationToken cancellationToken);
        public Task<MealLog> AddMealLog(MealLog mealLog, CancellationToken cancellationToken);
        public Task<MealLog> UpdateMealLog(MealLog fodmapLog, CancellationToken cancellationToken);
        public Task<MealLog> DeleteMealLog(int id, CancellationToken cancellationToken);
        public Task<IEnumerable<Product>> GetAllProducts(CancellationToken cancellationToken);
        public Task<Product> GetProductById(int id, CancellationToken cancellationToken);
        public Task<Product> AddProduct(Product product, CancellationToken cancellationToken);
        public Task<Product> UpdateProduct(Product product, CancellationToken cancellationToken);
        public Task<Product> DeleteProduct(int id, CancellationToken cancellationToken);

        public Task<Product> GetProductByExternalId(string externalId, CancellationToken cancellationToken);

        public Task<IEnumerable<MealLog>> GetMealLogsByDate(DateTime date, CancellationToken cancellationToken);
        public Task<IEnumerable<SymptomsLog>> GetSymptomsLogsByDate(DateTime date, CancellationToken cancellationToken);
        public Task<SymptomsLog> AddSymptomsLog(SymptomsLog symptomsLog, CancellationToken cancellationToken);
    }
}
