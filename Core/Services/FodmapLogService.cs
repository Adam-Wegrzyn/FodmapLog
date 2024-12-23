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
    public class FodmapLogService : IFodmapLogService
    {
        private readonly IFodmapLogRepository _fodmapLogRepository;
        private readonly IMapper _mapper;

        public FodmapLogService(IFodmapLogRepository fodmapLogRepository,
            IMapper autoMapper)
        {
            _fodmapLogRepository = fodmapLogRepository;
            _mapper = autoMapper;
            
        }

        public async Task<MealDto> AddMeal(MealDto Meal, CancellationToken cancellationToken)
        {
            var meal = _mapper.Map<Meal>(Meal);
            var addedMeal = await _fodmapLogRepository.AddMeal(meal);
            return _mapper.Map<MealDto>(addedMeal);
        }

        public async Task<ProductDto> AddProduct(ProductDto productDto, CancellationToken cancellationToken)
        {
            var product = _mapper.Map<Product>(productDto);
            var addedProduct = await _fodmapLogRepository.AddProduct(product);
            return _mapper.Map<ProductDto>(addedProduct);
        }

        public async Task<MealDto> DeleteMeal(int id, CancellationToken cancellationToken)
        {
            var deletedMeal = await _fodmapLogRepository.DeleteMeal(id);
            return _mapper.Map<MealDto>(deletedMeal);
        }

        public async Task<ProductDto> DeleteProduct(int id, CancellationToken cancellationToken)
        {
            var deletedProduct = await _fodmapLogRepository.DeleteProduct(id);
            return _mapper.Map<ProductDto>(deletedProduct);
        }

        public async Task<IEnumerable<MealDto>> GetAllMeals(CancellationToken cancellationToken)
        {
            var meals = await _fodmapLogRepository.GetAllMeals();
            return _mapper.Map<IEnumerable<MealDto>>(meals);
        }

        public async Task<IEnumerable<ProductDto>> GetAllProducts(CancellationToken cancellationToken)
        {
            var products = await _fodmapLogRepository.GetAllProducts();
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task<MealDto> GetMeal(int id, CancellationToken cancellationToken)
        {
            var meal = await _fodmapLogRepository.GetMeal(id);
            return _mapper.Map<MealDto>(meal);
        }

        public async Task<ProductDto> GetProduct(int id, CancellationToken cancellationToken)
        {
            var product = await _fodmapLogRepository.GetProduct(id);
            return _mapper.Map<ProductDto>(product);
        }

        public async Task<MealDto> UpdateMeal(MealDto mealDto, CancellationToken cancellationToken)
        {
            var meal = _mapper.Map<Meal>(mealDto);
            var updatedMeal = await _fodmapLogRepository.UpdateMeal(meal);
            return _mapper.Map<MealDto>(updatedMeal);
        }

        public async Task<ProductDto> UpdateProduct(ProductDto productDto, CancellationToken cancellationToken)
        {
            var product = _mapper.Map<Product>(productDto);
            var updatedProduct = await _fodmapLogRepository.UpdateProduct(product);
            return _mapper.Map<ProductDto>(updatedProduct);
        }
    }
}
