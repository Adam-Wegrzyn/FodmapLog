using Data.Common.DTO;
using DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IFodmapLogService
    {
        Task<MealLogDto?> GetMealLogById(int id, string userId, CancellationToken cancellationToken);
        Task<IEnumerable<MealLogDto>> GetAllMealLogs(string userId, CancellationToken cancellationToken);
        Task<MealLogDto> AddMealLog(MealLogDto mealLogDto, string userId, CancellationToken cancellationToken);
        Task<MealLogDto> UpdateMealLog(MealLogDto mealLogDto, string userId, CancellationToken cancellationToken);
        Task<MealLogDto?> DeleteMealLog(int id, string userId, CancellationToken cancellationToken);

        Task<ProductDto> GetProductById(int id, CancellationToken cancellationToken);
        Task<IEnumerable<ProductDto>> GetAllProducts(CancellationToken cancellationToken);
        Task<ProductDto> AddProduct(ProductDto productDto, CancellationToken cancellationToken);
        Task<ProductDto> UpdateProduct(ProductDto productDto, CancellationToken cancellationToken);
        Task<ProductDto> DeleteProduct(int id, CancellationToken cancellationToken);

        Task<IEnumerable<DailyLogDto>> GetDailyLogsByDate(DateTime date, string userId, CancellationToken cancellationToken);
        Task<SymptomsLogDto> AddSymptomsLog(SymptomsLogDto symptomsLogDto, string userId, CancellationToken cancellationToken);
        Task<SymptomsLogDto?> GetSymptomsLogById(int id, string userId, CancellationToken cancellationToken);
        Task<SymptomsLogDto?> UpdateSymptomsLog(SymptomsLogDto symptomsLogDto, string userId, CancellationToken cancellationToken);
    }
}
