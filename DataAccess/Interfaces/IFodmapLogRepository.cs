using DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DataAccess.Interfaces
{
    public interface IFodmapLogRepository
    {
        Task<IEnumerable<MealLog>> GetAllMealLogs(string userId, CancellationToken cancellationToken);
        Task<MealLog?> GetMealLogById(int id, string userId, CancellationToken cancellationToken);
        Task<MealLog> AddMealLog(MealLog mealLog, CancellationToken cancellationToken);
        Task<MealLog> UpdateMealLog(MealLog fodmapLog, string userId, CancellationToken cancellationToken);
        Task<MealLog?> DeleteMealLog(int id, string userId, CancellationToken cancellationToken);

        Task<IEnumerable<Product>> GetAllProducts(CancellationToken cancellationToken);
        Task<Product> GetProductById(int id, CancellationToken cancellationToken);
        Task<Product> AddProduct(Product product, CancellationToken cancellationToken);
        Task<Product> UpdateProduct(Product product, CancellationToken cancellationToken);
        Task<Product> DeleteProduct(int id, CancellationToken cancellationToken);

        Task<IEnumerable<MealLog>> GetMealLogsByDate(DateTime date, string userId, CancellationToken cancellationToken);
        Task<IEnumerable<SymptomsLog>> GetSymptomsLogsByDate(DateTime date, string userId, CancellationToken cancellationToken);
        Task<SymptomsLog> AddSymptomsLog(SymptomsLog symptomsLog, CancellationToken cancellationToken);
        Task<SymptomsLog?> GetSymptomsLogById(int id, string userId, CancellationToken cancellationToken);
        Task<SymptomsLog> UpdateSymptomsLog(SymptomsLog symptomsLog, string userId, CancellationToken cancellationToken);
        Task<IEnumerable<SymptomType>> GetAllSymptomTypes(CancellationToken cancellationToken);
        Task<IEnumerable<Unit>> GetAllUnits(CancellationToken cancellationToken);
    }
}
