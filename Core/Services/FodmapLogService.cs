using Core.Interfaces;
using DataAccess.Entities;
using DataAccess.Interfaces;
using DataAccess.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Services
{
    public class FodmapLogService : IFodmapLogService
    {
        private readonly IFodmapLogRepository _fodmapLogRepository;

        public FodmapLogService(IFodmapLogRepository fodmapLogRepository)
        {
            _fodmapLogRepository = fodmapLogRepository;
            
        }
        public async Task<MealLog> AddMealLog(MealLog mealLog, CancellationToken cancellationToken)
        {
            return await _fodmapLogRepository.AddMealLog(mealLog);
        }

        public async Task<Product> AddProduct(Product product, CancellationToken cancellationToken)
        {
            return await _fodmapLogRepository.AddProduct(product);
        }

        public async Task<MealLog> DeleteMealLog(int id, CancellationToken cancellationToken)
        {
            return await _fodmapLogRepository.DeleteMealLog(id);
        }

        public async Task<Product> DeleteProduct(int id, CancellationToken cancellationToken)
        {
            return await _fodmapLogRepository.DeleteProduct(id);
        }

        public async Task<IEnumerable<MealLog>> GetAllMealLogs(CancellationToken cancellationToken)
        {
            return await _fodmapLogRepository.GetAllMealLogs();
        }

        public async Task<IEnumerable<Product>> GetAllProducts(CancellationToken cancellationToken)
        {
            return await _fodmapLogRepository.GetAllProducts();
        }

        public async Task<MealLog> GetMealLog(int id, CancellationToken cancellationToken)
        {
            return await _fodmapLogRepository.GetMealLog(id);
        }

        public async Task<Product> GetProduct(int id, CancellationToken cancellationToken)
        {
            return await _fodmapLogRepository.GetProduct(id);
        }

        public async Task<MealLog> UpdateMealLog(MealLog mealLog, CancellationToken cancellationToken)
        {
            return await _fodmapLogRepository.UpdateMealLog(mealLog);
        }

        public async Task<Product> UpdateProduct(Product product, CancellationToken cancellationToken)
        {
            return await _fodmapLogRepository.UpdateProduct(product);
        }
    }
}
