using AutoMapper;
using Azure.Messaging.ServiceBus;
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
        private readonly ServiceBusClient _serviceBusClient;

        public FodmapLogService(IFodmapLogRepository fodmapLogRepository,
            IMapper autoMapper,
            ServiceBusClient serviceBusClient)
        {
            _fodmapLogRepository = fodmapLogRepository;
            _mapper = autoMapper;
            _serviceBusClient = serviceBusClient;
            _serviceBusClient = serviceBusClient;
        }

        public async Task<MealLogDto> AddMealLog(MealLogDto mealLogDto, CancellationToken cancellationToken)
        {
            var mealLog = _mapper.Map<MealLog>(mealLogDto);
            var addedMealLog = await _fodmapLogRepository.AddMealLog(mealLog, cancellationToken);

            var sender = _serviceBusClient.CreateSender("main-queue");
            await sender.SendMessageAsync(new ServiceBusMessage("New mealLog has been added!"));
            return _mapper.Map<MealLogDto>(addedMealLog);
        }

        public async Task<ProductDto> AddProduct(ProductDto productDto, CancellationToken cancellationToken)
        {
            var product = _mapper.Map<Product>(productDto);
            var addedProduct = await _fodmapLogRepository.AddProduct(product, cancellationToken);
            return _mapper.Map<ProductDto>(addedProduct);
        }

        public async Task<SymptomsLogDto> AddSymptomsLog(SymptomsLogDto symptomsLogDto, CancellationToken cancellationToken)
        {
            var symptomsLog = _mapper.Map<SymptomsLog>(symptomsLogDto);
            var addedSymptomsLog = await _fodmapLogRepository.AddSymptomsLog(symptomsLog, cancellationToken);
            return _mapper.Map<SymptomsLogDto>(addedSymptomsLog);
        }

        public async Task<MealLogDto> DeleteMealLog(int id, CancellationToken cancellationToken)
        {
            var deletedMealLog = await _fodmapLogRepository.DeleteMealLog(id, cancellationToken);
            return _mapper.Map<MealLogDto>(deletedMealLog);
        }

        public async Task<ProductDto> DeleteProduct(int id, CancellationToken cancellationToken)
        {
            var deletedProduct = await _fodmapLogRepository.DeleteProduct(id, cancellationToken);
            return _mapper.Map<ProductDto>(deletedProduct);
        }

        public async Task<IEnumerable<MealLogDto>> GetAllMealLogs(CancellationToken cancellationToken)
        {
            var mealLogs = await _fodmapLogRepository.GetAllMealLogs(cancellationToken);
            return _mapper.Map<IEnumerable<MealLogDto>>(mealLogs);
        }

        public async Task<IEnumerable<ProductDto>> GetAllProducts(CancellationToken cancellationToken)
        {
            var products = await _fodmapLogRepository.GetAllProducts(cancellationToken);
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task<IEnumerable<DailyLogDto>> GetDailyLogsByDate(DateTime date, CancellationToken cancellationToken)
        {
            var dailyLogs = new List<DailyLogDto>();
            var mealLogs = await _fodmapLogRepository.GetMealLogsByDate(date, cancellationToken);
            var symptomsLogs = await _fodmapLogRepository.GetSymptomsLogsByDate(date, cancellationToken);

            foreach (var mealLog in mealLogs)
            {
                dailyLogs.Add(new DailyLogDto
                {
                    Date = mealLog.Date,
                    MealLog = _mapper.Map<MealLogDto>(mealLog)
                });
            }
            foreach (var symptomLog in symptomsLogs)
            {
                dailyLogs.Add(new DailyLogDto
                {
                    Date = symptomLog.Date,
                    SymptomsLog = _mapper.Map<SymptomsLogDto>(symptomLog)
                });

            }
            return dailyLogs;
        }

        public async Task<MealLogDto> GetMealLogById(int id, CancellationToken cancellationToken)
        {
            var mealLog = await _fodmapLogRepository.GetMealLogById(id, cancellationToken);
            return _mapper.Map<MealLogDto>(mealLog);
        }

        public async Task<ProductDto> GetProductById(int id, CancellationToken cancellationToken)
        {
            var product = await _fodmapLogRepository.GetProductById(id, cancellationToken);
            return _mapper.Map<ProductDto>(product);
        }

        public async Task<SymptomsLog> GetSymptomsLogById(int id, CancellationToken cancellationToken)
        {
            var symptomsLog = await _fodmapLogRepository.GetSymptomsLogById(id, cancellationToken);
            return _mapper.Map<SymptomsLog>(symptomsLog);
        }

        public async Task<MealLogDto> UpdateMealLog(MealLogDto mealLogDto, CancellationToken cancellationToken)
        {
            var mealLog = _mapper.Map<MealLog>(mealLogDto);
            var updatedMealLog = await _fodmapLogRepository.UpdateMealLog(mealLog, cancellationToken);
            return _mapper.Map<MealLogDto>(updatedMealLog);
        }

        public async Task<ProductDto> UpdateProduct(ProductDto productDto, CancellationToken cancellationToken)
        {
            var product = _mapper.Map<Product>(productDto);
            var updatedProduct = await _fodmapLogRepository.UpdateProduct(product, cancellationToken);
            return _mapper.Map<ProductDto>(updatedProduct);
        }


        public async Task<SymptomsLog> UpdateSymptomsLog(SymptomsLogDto symptomsLogDto, CancellationToken cancellationToken)
        {
            var symptomsLog = _mapper.Map<SymptomsLog>(symptomsLogDto);
            var updatedSymptomsLog = await _fodmapLogRepository.UpdateSymptomsLog(symptomsLog, cancellationToken);
            return _mapper.Map<SymptomsLog>(updatedSymptomsLog);
        }


    }
}
