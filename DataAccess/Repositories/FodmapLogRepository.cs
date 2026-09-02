using DataAccess.Entities;
using DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
    public class FodmapLogRepository : IFodmapLogRepository
    {
        private readonly FodmapLogDbContext _context;

        public FodmapLogRepository(FodmapLogDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<MealLog>> GetAllMealLogs(string userId, CancellationToken cancellationToken)
        {
            return await _context.MealLogs
                .Where(m => m.UserId == userId)
                .Include(m => m.ProductQuantity)
                .ThenInclude(pq => pq.Product)
                .ToListAsync(cancellationToken);
        }

        public async Task<MealLog?> GetMealLogById(int id, string userId, CancellationToken cancellationToken)
        {
            return await _context.MealLogs
                .Include(m => m.ProductQuantity)
                .ThenInclude(pq => pq.Product)
                .Include(m => m.ProductQuantity)
                .ThenInclude(pq => pq.Unit)
                .SingleOrDefaultAsync(m => m.Id == id && m.UserId == userId, cancellationToken);
        }

        public async Task<IEnumerable<MealLog>> GetMealLogsByDate(DateTime date, string userId, CancellationToken cancellationToken)
        {
            return await _context.MealLogs
                .Include(m => m.ProductQuantity)
                .ThenInclude(pq => pq.Product)
                .Include(m => m.ProductQuantity)
                .ThenInclude(pq => pq.Unit)
                .Where(m => m.UserId == userId && m.Date.Date == date.Date)
                .ToListAsync(cancellationToken);
        }

        public async Task<MealLog> AddMealLog(MealLog mealLog, CancellationToken cancellationToken)
        {
            foreach (var productQuantity in mealLog.ProductQuantity)
            {
                var existingUnit = await _context.Units
                    .FirstOrDefaultAsync(u => u.Id == productQuantity.Unit.Id, cancellationToken);
                if (existingUnit == null)
                {
                    await AddUnit(productQuantity.Unit, cancellationToken);
                }
                else
                {
                    productQuantity.Unit = existingUnit;
                }
            }
            _context.MealLogs.Add(mealLog);
            await _context.SaveChangesAsync(cancellationToken);
            return mealLog;
        }

        private async Task AddUnit(Unit? unit, CancellationToken cancellationToken)
        {
            _context.Units.Add(unit);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<MealLog> UpdateMealLog(MealLog updatedMealLog, string userId, CancellationToken cancellationToken)
        {
            var existingMealLog = await _context.MealLogs
                .Include(m => m.ProductQuantity)
                .ThenInclude(pq => pq.Product)
                .FirstOrDefaultAsync(m => m.Id == updatedMealLog.Id && m.UserId == userId, cancellationToken);

            if (existingMealLog == null)
            {
                throw new InvalidOperationException("MealLog not found");
            }

            updatedMealLog.UserId = userId;
            _context.Entry(existingMealLog).CurrentValues.SetValues(updatedMealLog);

            foreach (var updatedProductQuantity in updatedMealLog.ProductQuantity)
            {
                var existingProductQuantity = existingMealLog.ProductQuantity
                    .FirstOrDefault(pq => pq.Id == updatedProductQuantity.Id);

                if (existingProductQuantity != null)
                {
                    _context.Entry(existingProductQuantity).CurrentValues.SetValues(updatedProductQuantity);
                }
                else
                {
                    existingMealLog.ProductQuantity.Add(updatedProductQuantity);
                }
            }

            foreach (var existingProductQuantity in existingMealLog.ProductQuantity.ToList())
            {
                if (!updatedMealLog.ProductQuantity.Any(pq => pq.Id == existingProductQuantity.Id))
                {
                    _context.ProductQuantities.Remove(existingProductQuantity);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
            return existingMealLog;
        }

        public async Task<MealLog?> DeleteMealLog(int id, string userId, CancellationToken cancellationToken)
        {
            var mealLog = await _context.MealLogs
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId, cancellationToken);
            if (mealLog != null)
            {
                _context.MealLogs.Remove(mealLog);
                await _context.SaveChangesAsync(cancellationToken);
            }
            return mealLog;
        }

        public async Task<IEnumerable<Product>> GetAllProducts(CancellationToken cancellationToken)
        {
            return await _context.Products.ToListAsync(cancellationToken);
        }

        public async Task<Product> GetProductById(int id, CancellationToken cancellationToken)
        {
            return await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        public async Task<Product> AddProduct(Product product, CancellationToken cancellationToken)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync(cancellationToken);
            return product;
        }

        public async Task<Product> UpdateProduct(Product product, CancellationToken cancellationToken)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync(cancellationToken);
            return product;
        }

        public async Task<Product> DeleteProduct(int id, CancellationToken cancellationToken)
        {
            var product = await _context.Products.FindAsync(new object[] { id }, cancellationToken);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync(cancellationToken);
            }
            return product;
        }

        public async Task<IEnumerable<SymptomsLog>> GetSymptomsLogsByDate(DateTime date, string userId, CancellationToken cancellationToken)
        {
            return await _context.SymptomsLogs
                .Include(s => s.Symptoms)
                .ThenInclude(s => s.SymptomType)
                .Where(s => s.UserId == userId && s.Date.Date == date.Date)
                .ToListAsync(cancellationToken);
        }

        public async Task<SymptomsLog> AddSymptomsLog(SymptomsLog symptomsLog, CancellationToken cancellationToken)
        {
            foreach (var symptom in symptomsLog.Symptoms)
            {
                var existingSymptomType = await _context.SymptomTypes
                    .FirstOrDefaultAsync(s => s.Id == symptom.SymptomType.Id, cancellationToken);
                if (existingSymptomType != null)
                {
                    symptom.SymptomType = existingSymptomType;
                }
                else
                {
                    await AddSymptomType(symptom.SymptomType, cancellationToken);
                }
            }
            _context.SymptomsLogs.Add(symptomsLog);
            await _context.SaveChangesAsync(cancellationToken);
            return symptomsLog;
        }

        private async Task AddSymptomType(SymptomType symptomType, CancellationToken cancellationToken)
        {
            _context.SymptomTypes.Add(symptomType);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<SymptomsLog?> GetSymptomsLogById(int id, string userId, CancellationToken cancellationToken)
        {
            return await _context.SymptomsLogs
                 .Include(s => s.Symptoms)
                 .ThenInclude(s => s.SymptomType)
                 .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId, cancellationToken);
        }

        public async Task<SymptomsLog> UpdateSymptomsLog(SymptomsLog symptomsLog, string userId, CancellationToken cancellationToken)
        {
            var existing = await _context.SymptomsLogs
                .FirstOrDefaultAsync(s => s.Id == symptomsLog.Id && s.UserId == userId, cancellationToken);

            if (existing == null)
            {
                throw new InvalidOperationException("SymptomsLog not found");
            }

            symptomsLog.UserId = userId;
            _context.SymptomsLogs.Update(symptomsLog);
            await _context.SaveChangesAsync(cancellationToken);
            return symptomsLog;
        }

        public async Task<IEnumerable<SymptomType>> GetAllSymptomTypes(CancellationToken cancellationToken)
        {
            return await _context.SymptomTypes.ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Unit>> GetAllUnits(CancellationToken cancellationToken)
        {
            return await _context.Units.ToListAsync(cancellationToken);
        }
    }
}
