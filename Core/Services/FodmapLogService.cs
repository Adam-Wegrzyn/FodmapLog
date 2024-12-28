using AutoMapper;
using Core.Interfaces;
using Data.Common.DTO;
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
    public class FodmapLogService :IFodmapLogService
    {
        private readonly IFodmapLogRepository _fodmapLogRepository;
        private readonly IMapper _mapper;

        public FodmapLogService(IFodmapLogRepository fodmapLogRepository,
            IMapper autoMapper)
        {
            _fodmapLogRepository = fodmapLogRepository;
            _mapper = autoMapper;
            
        }

        public async Task<MealLogDto> AddMealLog(MealLogDto mealLogDto, CancellationToken cancellationToken)
        {
            var mealLog = _mapper.Map<MealLog>(mealLogDto);
            var addedMealLog = await _fodmapLogRepository.AddMealLog(mealLog);
            return _mapper.Map<MealLogDto>(addedMealLog);
        }

        public async Task<ProductDto> AddProduct(ProductDto productDto, CancellationToken cancellationToken)
        {
            var product = _mapper.Map<Product>(productDto);
            var addedProduct = await _fodmapLogRepository.AddProduct(product);
            return _mapper.Map<ProductDto>(addedProduct);
        }

        public async Task<MealLogDto> DeleteMealLog(int id, CancellationToken cancellationToken)
        {
            var deletedMealLog = await _fodmapLogRepository.DeleteMealLog(id);
            return _mapper.Map<MealLogDto>(deletedMealLog);
        }

        public async Task<ProductDto> DeleteProduct(int id, CancellationToken cancellationToken)
        {
            var deletedProduct = await _fodmapLogRepository.DeleteProduct(id);
            return _mapper.Map<ProductDto>(deletedProduct);
        }

        public async Task<IEnumerable<MealLogDto>> GetAllMealLogs(CancellationToken cancellationToken)
        {
            var mealLogs = await _fodmapLogRepository.GetAllMealLogs();
            return _mapper.Map<IEnumerable<MealLogDto>>(mealLogs);
        }

        public async Task<IEnumerable<ProductDto>> GetAllProducts(CancellationToken cancellationToken)
        {
            var products = await _fodmapLogRepository.GetAllProducts();
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task<MealLogDto> GetMealLogById(int id, CancellationToken cancellationToken)
        {
            var mealLog = await _fodmapLogRepository.GetMealLogById(id);
            return _mapper.Map<MealLogDto>(mealLog);
        }

        public async Task<ProductDto> GetProductById(int id, CancellationToken cancellationToken)
        {
            var product = await _fodmapLogRepository.GetProductById(id);
            return _mapper.Map<ProductDto>(product);
        }

        public async Task<MealLogDto> UpdateMealLog(MealLogDto mealLogDto, CancellationToken cancellationToken)
        {
            var mealLog = _mapper.Map<MealLog>(mealLogDto);
            var updatedMealLog = await _fodmapLogRepository.UpdateMealLog(mealLog);
            return _mapper.Map<MealLogDto>(updatedMealLog);
        }

        public async Task<ProductDto> UpdateProduct(ProductDto productDto, CancellationToken cancellationToken)
        {
            var product = _mapper.Map<Product>(productDto);
            var updatedProduct = await _fodmapLogRepository.UpdateProduct(product);
            return _mapper.Map<ProductDto>(updatedProduct);
        }
    }
}
