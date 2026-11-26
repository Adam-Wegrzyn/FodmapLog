using AutoMapper;
using Core.Ai;
using Core.Interfaces;
using Data.Common.DTO;
using DataAccess.Entities;
using DataAccess.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Services
{
    public class FodmapLogService : IFodmapLogService
    {
        private readonly IFodmapLogRepository _fodmapLogRepository;
        private readonly IMapper _mapper;
        private readonly IUserClock _clock;

        public FodmapLogService(
            IFodmapLogRepository fodmapLogRepository,
            IMapper autoMapper,
            IUserClock? clock = null)
        {
            _fodmapLogRepository = fodmapLogRepository;
            _mapper = autoMapper;
            _clock = clock ?? UserClock.Unknown;
        }

        public async Task<MealLogDto> AddMealLog(MealLogDto mealLogDto, string userId, CancellationToken cancellationToken)
        {
            var mealLog = _mapper.Map<MealLog>(mealLogDto);
            mealLog.UserId = userId;
            StoreInstant(mealLog);
            var addedMealLog = await _fodmapLogRepository.AddMealLog(mealLog, cancellationToken);
            return Present(addedMealLog);
        }

        public async Task<ProductDto> AddProduct(ProductDto productDto, CancellationToken cancellationToken)
        {
            var product = _mapper.Map<Product>(productDto);
            var addedProduct = await _fodmapLogRepository.AddProduct(product, cancellationToken);
            return _mapper.Map<ProductDto>(addedProduct);
        }

        public async Task<SymptomsLogDto> AddSymptomsLog(SymptomsLogDto symptomsLogDto, string userId, CancellationToken cancellationToken)
        {
            var symptomsLog = _mapper.Map<SymptomsLog>(symptomsLogDto);
            symptomsLog.UserId = userId;
            StoreInstant(symptomsLog);
            var addedSymptomsLog = await _fodmapLogRepository.AddSymptomsLog(symptomsLog, cancellationToken);
            return Present(addedSymptomsLog);
        }

        public async Task<MealLogDto?> DeleteMealLog(int id, string userId, CancellationToken cancellationToken)
        {
            var deletedMealLog = await _fodmapLogRepository.DeleteMealLog(id, userId, cancellationToken);
            return deletedMealLog == null ? null : Present(deletedMealLog);
        }

        public async Task<ProductDto> DeleteProduct(int id, CancellationToken cancellationToken)
        {
            var deletedProduct = await _fodmapLogRepository.DeleteProduct(id, cancellationToken);
            return _mapper.Map<ProductDto>(deletedProduct);
        }

        public async Task<IEnumerable<MealLogDto>> GetAllMealLogs(string userId, CancellationToken cancellationToken)
        {
            var mealLogs = await _fodmapLogRepository.GetAllMealLogs(userId, cancellationToken);
            return mealLogs.Select(Present).ToList();
        }

        public async Task<IEnumerable<ProductDto>> GetAllProducts(CancellationToken cancellationToken)
        {
            var products = await _fodmapLogRepository.GetAllProducts(cancellationToken);
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task<IEnumerable<DailyLogDto>> GetDailyLogsByDate(DateTime date, string userId, CancellationToken cancellationToken)
        {
            var dailyLogs = new List<DailyLogDto>();
            var (utcStart, utcEnd) = UtcBounds(date, date);
            var mealLogs = await _fodmapLogRepository.GetMealLogsByDate(date, userId, cancellationToken, utcStart, utcEnd);
            var symptomsLogs = await _fodmapLogRepository.GetSymptomsLogsByDate(date, userId, cancellationToken, utcStart, utcEnd);

            foreach (var mealLog in mealLogs)
            {
                var meal = Present(mealLog);
                dailyLogs.Add(new DailyLogDto
                {
                    Date = meal.Date,
                    MealLog = meal
                });
            }
            foreach (var symptomLog in symptomsLogs)
            {
                var symptoms = Present(symptomLog);
                dailyLogs.Add(new DailyLogDto
                {
                    Date = symptoms.Date,
                    SymptomsLog = symptoms
                });
            }
            return dailyLogs.OrderBy(d => d.Date).ToList();
        }

        public async Task<IEnumerable<DailyLogDto>> GetDailyLogsByDateRange(DateTime fromDate, DateTime toDate, string userId, CancellationToken cancellationToken)
        {
            var dailyLogs = new List<DailyLogDto>();
            var (utcStart, utcEnd) = UtcBounds(fromDate, toDate);
            var mealLogs = await _fodmapLogRepository.GetMealLogsByDateRange(fromDate, toDate, userId, cancellationToken, utcStart, utcEnd);
            var symptomsLogs = await _fodmapLogRepository.GetSymptomsLogsByDateRange(fromDate, toDate, userId, cancellationToken, utcStart, utcEnd);

            foreach (var mealLog in mealLogs)
            {
                var meal = Present(mealLog);
                dailyLogs.Add(new DailyLogDto
                {
                    Date = meal.Date,
                    MealLog = meal
                });
            }
            foreach (var symptomLog in symptomsLogs)
            {
                var symptoms = Present(symptomLog);
                dailyLogs.Add(new DailyLogDto
                {
                    Date = symptoms.Date,
                    SymptomsLog = symptoms
                });
            }

            return dailyLogs.OrderBy(d => d.Date).ToList();
        }

        public async Task<MealLogDto?> GetMealLogById(int id, string userId, CancellationToken cancellationToken)
        {
            var mealLog = await _fodmapLogRepository.GetMealLogById(id, userId, cancellationToken);
            return mealLog == null ? null : Present(mealLog);
        }

        public async Task<ProductDto> GetProductById(int id, CancellationToken cancellationToken)
        {
            var product = await _fodmapLogRepository.GetProductById(id, cancellationToken);
            return _mapper.Map<ProductDto>(product);
        }

        public async Task<SymptomsLogDto?> GetSymptomsLogById(int id, string userId, CancellationToken cancellationToken)
        {
            var symptomsLog = await _fodmapLogRepository.GetSymptomsLogById(id, userId, cancellationToken);
            return symptomsLog == null ? null : Present(symptomsLog);
        }

        public async Task<MealLogDto> UpdateMealLog(MealLogDto mealLogDto, string userId, CancellationToken cancellationToken)
        {
            var mealLog = _mapper.Map<MealLog>(mealLogDto);
            mealLog.UserId = userId;
            StoreInstant(mealLog);
            var updatedMealLog = await _fodmapLogRepository.UpdateMealLog(mealLog, userId, cancellationToken);
            return Present(updatedMealLog);
        }

        public async Task<ProductDto> UpdateProduct(ProductDto productDto, CancellationToken cancellationToken)
        {
            var product = _mapper.Map<Product>(productDto);
            var updatedProduct = await _fodmapLogRepository.UpdateProduct(product, cancellationToken);
            return _mapper.Map<ProductDto>(updatedProduct);
        }

        public async Task<SymptomsLogDto?> UpdateSymptomsLog(SymptomsLogDto symptomsLogDto, string userId, CancellationToken cancellationToken)
        {
            var symptomsLog = _mapper.Map<SymptomsLog>(symptomsLogDto);
            symptomsLog.UserId = userId;
            StoreInstant(symptomsLog);
            var updatedSymptomsLog = await _fodmapLogRepository.UpdateSymptomsLog(symptomsLog, userId, cancellationToken);
            return Present(updatedSymptomsLog);
        }

        public async Task<SymptomsLogDto?> DeleteSymptomsLog(int id, string userId, CancellationToken cancellationToken)
        {
            var deleted = await _fodmapLogRepository.DeleteSymptomsLog(id, userId, cancellationToken);
            return deleted == null ? null : Present(deleted);
        }

        private (DateTime? Start, DateTime? End) UtcBounds(DateTime from, DateTime to)
        {
            if (!_clock.IsKnown)
            {
                return (null, null);
            }

            var start = _clock.UtcWindow(from).UtcStart;
            var end = _clock.UtcWindow(to).UtcEnd;
            return (start, end);
        }

        private void StoreInstant(MealLog mealLog)
        {
            if (!_clock.IsKnown)
            {
                return;
            }

            mealLog.Date = _clock.ToUtc(mealLog.Date);
            mealLog.UtcInstant = true;
        }

        private void StoreInstant(SymptomsLog symptomsLog)
        {
            if (!_clock.IsKnown)
            {
                return;
            }

            symptomsLog.Date = _clock.ToUtc(symptomsLog.Date);
            symptomsLog.UtcInstant = true;
        }

        private MealLogDto Present(MealLog mealLog)
        {
            var dto = _mapper.Map<MealLogDto>(mealLog);
            dto.Date = PresentDate(mealLog.Date, mealLog.UtcInstant);
            return dto;
        }

        private SymptomsLogDto Present(SymptomsLog symptomsLog)
        {
            var dto = _mapper.Map<SymptomsLogDto>(symptomsLog);
            dto.Date = PresentDate(symptomsLog.Date, symptomsLog.UtcInstant);
            return dto;
        }

        private DateTime PresentDate(DateTime stored, bool utcInstant)
        {
            var civil = utcInstant ? _clock.ToCivil(stored) : stored;
            return DateTime.SpecifyKind(civil, DateTimeKind.Unspecified);
        }

        public Task DeleteAllLogsForUser(string userId, CancellationToken cancellationToken)
        {
            return _fodmapLogRepository.DeleteAllLogsForUser(userId, cancellationToken);
        }
    }
}
