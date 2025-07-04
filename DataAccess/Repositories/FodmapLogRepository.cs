using DataAccess.Entities;
using DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DataAccess.Repositories.FodmapLogRepository;

namespace DataAccess.Repositories
{
    public class FodmapLogRepository : IFodmapLogRepository
    {
        private readonly FodmapLogDbContext _context;

        public FodmapLogRepository(FodmapLogDbContext context)
        {
            _context = context;
        }

        // Methods for MealLog
        public async Task<IEnumerable<MealLog>> GetAllMealLogs(CancellationToken cancellationToken)
        {
            return await _context.MealLogs
                .Include(m => m.ProductQuantity)
                .ThenInclude(pq => pq.Product)
                .ToListAsync();
        }

        public async Task<MealLog> GetMealLogById(int id, CancellationToken cancellationToken)
        {
            return await _context.MealLogs
                .Include(m => m.ProductQuantity)
                .ThenInclude(pq => pq.Product)
                .Include(m => m.ProductQuantity)
                .ThenInclude(pq => pq.Unit)
                .SingleOrDefaultAsync(m => m.Id == id);
        }
        public async Task<IEnumerable<MealLog>> GetMealLogsByDate(DateTime date, CancellationToken cancellationToken)
        {
            return await _context.MealLogs
                .Include(m => m.ProductQuantity)
                .ThenInclude(pq => pq.Product)
                                .Include(m => m.ProductQuantity)
                .ThenInclude(pq => pq.Unit)
                .Where(m => m.Date.Date == date.Date)
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

        public async Task<MealLog> UpdateMealLog(MealLog updatedMealLog, CancellationToken cancellationToken)
        {

            // Retrieve the existing MealLog from the database
            var existingMealLog = await _context.MealLogs
                .Include(m => m.ProductQuantity)
                .ThenInclude(pq => pq.Product)
                .FirstOrDefaultAsync(m => m.Id == updatedMealLog.Id, cancellationToken);

            if (existingMealLog == null)
            {
                throw new InvalidOperationException("MealLog not found");
            }

            // Update the main entity properties
            _context.Entry(existingMealLog).CurrentValues.SetValues(updatedMealLog);

            // Update nested entities (ProductQuantity)
            foreach (var updatedProductQuantity in updatedMealLog.ProductQuantity)
            {
                var existingProductQuantity = existingMealLog.ProductQuantity
                    .FirstOrDefault(pq => pq.Id == updatedProductQuantity.Id);

                if (existingProductQuantity != null)
                {
                    // Update existing ProductQuantity
                    _context.Entry(existingProductQuantity).CurrentValues.SetValues(updatedProductQuantity);
                }
                else
                {
                    // Add new ProductQuantity
                    existingMealLog.ProductQuantity.Add(updatedProductQuantity);
                }
            }

            // Remove ProductQuantity that are no longer present
            foreach (var existingProductQuantity in existingMealLog.ProductQuantity.ToList())
            {
                if (!updatedMealLog.ProductQuantity.Any(pq => pq.Id == existingProductQuantity.Id))
                {
                    _context.ProductQuantities.Remove(existingProductQuantity);
                }
            }
            //existingMealLog.ProductQuantity = updatedMealLog.ProductQuantity;

            await _context.SaveChangesAsync();
            return existingMealLog;
        }

        public async Task<MealLog> DeleteMealLog(int id, CancellationToken cancellationToken)
        {
            var MealLog = await _context.MealLogs.FindAsync(id);
            if (MealLog != null)
            {
                _context.MealLogs.Remove(MealLog);
                await _context.SaveChangesAsync();
            }
            return MealLog;
        }

        // Methods for Product
        public async Task<IEnumerable<Product>> GetAllProducts(CancellationToken cancellationToken)
        {
            return await _context.Products.ToListAsync();
        }

        public async Task<Product> GetProductById(int id, CancellationToken cancellationToken)
        {
            return await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id);
        }


        public async Task<Product> AddProduct(Product product, CancellationToken cancellationToken)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<Product> UpdateProduct(Product product, CancellationToken cancellationToken)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<Product> DeleteProduct(int id, CancellationToken cancellationToken)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
            return product;
        }

        //methods for symptoms

        public async Task<IEnumerable<SymptomsLog>> GetSymptomsLogsByDate(DateTime date, CancellationToken cancellationToken)
        {
            return await _context.SymptomsLogs
                .Include(s => s.Symptoms)
                .ThenInclude(s => s.SymptomType)
                .Where(s => s.Date.Date == date.Date)
                .ToListAsync(cancellationToken);
        }

        public async Task<SymptomsLog> AddSymptomsLog(SymptomsLog symptomsLog, CancellationToken cancellationToken)
        {
            //get the existing symptoms from the database
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
            await _context.SaveChangesAsync();
            return symptomsLog;
        }
        private async Task AddSymptomType(SymptomType symptomType, CancellationToken cancellationToken)
        {
            _context.SymptomTypes.Add(symptomType);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<SymptomsLog> GetSymptomsLogById(int id, CancellationToken cancellationToken)
        {
            return await _context.SymptomsLogs
                 .Include(s => s.Symptoms)
                 .ThenInclude(s => s.SymptomType)
                 .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<SymptomsLog> UpdateSymptomsLog(SymptomsLog symptomsLog, CancellationToken cancellationToken)
        {
            _context.SymptomsLogs.Update(symptomsLog);
            await _context.SaveChangesAsync();
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

