using DataAccess.Entities;
using DataAccess.Enums;
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
            var dayStart = date.Date;
            var dayEnd = dayStart.AddDays(1);
            return await _context.MealLogs
                .Include(m => m.ProductQuantity)
                .ThenInclude(pq => pq.Product)
                .Include(m => m.ProductQuantity)
                .ThenInclude(pq => pq.Unit)
                .Where(m => m.UserId == userId && m.Date >= dayStart && m.Date < dayEnd)
                .ToListAsync(cancellationToken);
        }

        public async Task<MealLog> AddMealLog(MealLog mealLog, CancellationToken cancellationToken)
        {
            foreach (var productQuantity in mealLog.ProductQuantity)
            {
                if (productQuantity.Unit == null)
                {
                    productQuantity.Unit = new Unit { Name = "Piece" };
                }

                Unit? existingUnit = null;
                if (productQuantity.Unit.Id > 0)
                {
                    existingUnit = await _context.Units
                        .FirstOrDefaultAsync(u => u.Id == productQuantity.Unit.Id, cancellationToken);
                }
                if (existingUnit == null && !string.IsNullOrWhiteSpace(productQuantity.Unit.Name))
                {
                    existingUnit = await _context.Units
                        .FirstOrDefaultAsync(u => u.Name.ToLower() == productQuantity.Unit.Name.ToLower(), cancellationToken);
                }
                if (existingUnit == null)
                {
                    await AddUnit(productQuantity.Unit, cancellationToken);
                }
                else
                {
                    productQuantity.Unit = existingUnit;
                }

                if (productQuantity.Product == null)
                {
                    productQuantity.Product = new Product { Name = "Unknown" };
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

        public async Task<SymptomsLog?> DeleteSymptomsLog(int id, string userId, CancellationToken cancellationToken)
        {
            var symptomsLog = await _context.SymptomsLogs
                .Include(s => s.Symptoms)
                .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId, cancellationToken);
            if (symptomsLog != null)
            {
                _context.SymptomsLogs.Remove(symptomsLog);
                await _context.SaveChangesAsync(cancellationToken);
            }
            return symptomsLog;
        }

        public async Task DeleteAllLogsForUser(string userId, CancellationToken cancellationToken)
        {
            var meals = await _context.MealLogs
                .Include(m => m.ProductQuantity)
                .Where(m => m.UserId == userId)
                .ToListAsync(cancellationToken);
            _context.MealLogs.RemoveRange(meals);

            var symptoms = await _context.SymptomsLogs
                .Include(s => s.Symptoms)
                .Where(s => s.UserId == userId)
                .ToListAsync(cancellationToken);
            _context.SymptomsLogs.RemoveRange(symptoms);

            var personalTypes = await _context.SymptomTypes
                .Where(t => t.UserId == userId)
                .ToListAsync(cancellationToken);
            _context.SymptomTypes.RemoveRange(personalTypes);

            await _context.SaveChangesAsync(cancellationToken);
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
            var dayStart = date.Date;
            var dayEnd = dayStart.AddDays(1);
            return await _context.SymptomsLogs
                .Include(s => s.Symptoms)
                .ThenInclude(s => s.SymptomType)
                .Where(s => s.UserId == userId && s.Date >= dayStart && s.Date < dayEnd)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<MealLog>> GetMealLogsByDateRange(DateTime fromDate, DateTime toDate, string userId, CancellationToken cancellationToken)
        {
            var rangeStart = fromDate.Date;
            var rangeEnd = toDate.Date.AddDays(1);
            return await _context.MealLogs
                .Include(m => m.ProductQuantity)
                .ThenInclude(pq => pq.Product)
                .Include(m => m.ProductQuantity)
                .ThenInclude(pq => pq.Unit)
                .Where(m => m.UserId == userId && m.Date >= rangeStart && m.Date < rangeEnd)
                .OrderBy(m => m.Date)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<SymptomsLog>> GetSymptomsLogsByDateRange(DateTime fromDate, DateTime toDate, string userId, CancellationToken cancellationToken)
        {
            var rangeStart = fromDate.Date;
            var rangeEnd = toDate.Date.AddDays(1);
            return await _context.SymptomsLogs
                .Include(s => s.Symptoms)
                .ThenInclude(s => s.SymptomType)
                .Where(s => s.UserId == userId && s.Date >= rangeStart && s.Date < rangeEnd)
                .OrderBy(s => s.Date)
                .ToListAsync(cancellationToken);
        }

        public async Task<SymptomsLog> AddSymptomsLog(SymptomsLog symptomsLog, CancellationToken cancellationToken)
        {
            foreach (var symptom in symptomsLog.Symptoms)
            {
                symptom.SymptomType = await ResolveSymptomType(
                    symptom.SymptomType,
                    symptomsLog.UserId,
                    cancellationToken);
            }
            _context.SymptomsLogs.Add(symptomsLog);
            await _context.SaveChangesAsync(cancellationToken);
            return symptomsLog;
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
                .Include(s => s.Symptoms)
                .FirstOrDefaultAsync(s => s.Id == symptomsLog.Id && s.UserId == userId, cancellationToken);

            if (existing == null)
            {
                throw new InvalidOperationException("SymptomsLog not found");
            }

            existing.Date = symptomsLog.Date;
            existing.UserId = userId;
            existing.Symptoms ??= new List<Symptom>();

            if (existing.Symptoms.Count > 0)
            {
                _context.Symptoms.RemoveRange(existing.Symptoms);
                existing.Symptoms.Clear();
            }

            foreach (var incoming in symptomsLog.Symptoms ?? new List<Symptom>())
            {
                var type = await ResolveSymptomType(incoming.SymptomType, userId, cancellationToken);
                existing.Symptoms.Add(new Symptom
                {
                    SymptomType = type,
                    SymptomScale = incoming.SymptomScale
                });
            }

            await _context.SaveChangesAsync(cancellationToken);
            return existing;
        }

        private async Task<SymptomType> ResolveSymptomType(
            SymptomType? symptomType,
            string? userId,
            CancellationToken cancellationToken)
        {
            var name = NormalizeSymptomTypeName(symptomType?.Name);

            if (symptomType?.Id > 0)
            {
                var byId = await _context.SymptomTypes
                    .FirstOrDefaultAsync(
                        s => s.Id == symptomType.Id
                             && (s.UserId == null || s.UserId == userId),
                        cancellationToken);
                if (byId != null)
                {
                    return byId;
                }
            }

            if (!string.IsNullOrEmpty(name))
            {
                var match = await FindVisibleSymptomType(name, userId, cancellationToken);
                if (match != null)
                {
                    return match;
                }

                if (!string.IsNullOrWhiteSpace(userId))
                {
                    return await InsertPersonalSymptomType(name, userId, cancellationToken);
                }
            }

            throw new ArgumentException("Symptom type is required.");
        }

        public async Task<IEnumerable<SymptomType>> GetAllSymptomTypes(string userId, CancellationToken cancellationToken)
        {
            return await _context.SymptomTypes
                .Where(t => t.UserId == null || t.UserId == userId)
                .OrderByDescending(t => t.UserId != null)
                .ThenBy(t => t.Id)
                .ToListAsync(cancellationToken);
        }

        public async Task<SymptomType> AddPersonalSymptomType(
            string name,
            string userId,
            CancellationToken cancellationToken,
            SymptomScaleKind scaleKind = SymptomScaleKind.Negative)
        {
            var normalized = NormalizeSymptomTypeName(name);
            if (string.IsNullOrEmpty(normalized) || string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("Symptom name is required.");
            }

            var existing = await FindVisibleSymptomType(normalized, userId, cancellationToken);
            if (existing != null)
            {
                return existing;
            }

            return await InsertPersonalSymptomType(normalized, userId, cancellationToken, scaleKind);
        }

        private async Task<SymptomType?> FindVisibleSymptomType(
            string name,
            string? userId,
            CancellationToken cancellationToken)
        {
            var matches = await _context.SymptomTypes
                .Where(s => s.Name.ToLower() == name.ToLower()
                            && (s.UserId == null || s.UserId == userId))
                .ToListAsync(cancellationToken);

            return matches.FirstOrDefault(s => s.UserId == null)
                ?? matches.FirstOrDefault();
        }

        private async Task<SymptomType> InsertPersonalSymptomType(
            string name,
            string userId,
            CancellationToken cancellationToken,
            SymptomScaleKind scaleKind = SymptomScaleKind.Negative)
        {
            var kind = scaleKind == SymptomScaleKind.Positive
                ? SymptomScaleKind.Positive
                : SymptomScaleKind.Negative;
            var created = new SymptomType { Name = name, UserId = userId, ScaleKind = kind };
            _context.SymptomTypes.Add(created);
            await _context.SaveChangesAsync(cancellationToken);
            return created;
        }

        internal static string NormalizeSymptomTypeName(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return string.Empty;
            }

            var trimmed = string.Join(
                ' ',
                name.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
            return trimmed.Length <= 80 ? trimmed : trimmed[..80];
        }

        public async Task<IEnumerable<Unit>> GetAllUnits(CancellationToken cancellationToken)
        {
            return await _context.Units.ToListAsync(cancellationToken);
        }
    }
}
