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
        Task<MealLogDto> GetMealLogById(int id, CancellationToken cancellationToken);
        Task<IEnumerable<MealLogDto>> GetAllMealLogs(CancellationToken cancellationToken);
        Task<MealLogDto> AddMealLog(MealLogDto MealLogDto, CancellationToken cancellationToken);
        Task<MealLogDto> UpdateMealLog(MealLogDto MealLogDto, CancellationToken cancellationToken);
        Task<MealLogDto> DeleteMealLog(int id, CancellationToken cancellationToken);

        // Methods for Product
        Task<ProductDto> GetProductById(int id, CancellationToken cancellationToken);
        Task<IEnumerable<ProductDto>> GetAllProducts(CancellationToken cancellationToken);
        Task<ProductDto> AddProduct(ProductDto productDto, CancellationToken cancellationToken);
        Task<ProductDto> UpdateProduct(ProductDto productDto, CancellationToken cancellationToken);
        Task<ProductDto> DeleteProduct(int id, CancellationToken cancellationToken);

        Task<IEnumerable<DailyLogDto>> GetDailyLogsByDate(DateTime date, CancellationToken cancellationToken);
        Task<SymptomsLogDto> AddSymptomsLog(SymptomsLogDto symptomsLogDto, CancellationToken cancellationToken);

    }
}
