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
            return await _context.MealLogs.FindAsync(id);
        }
        public async Task<IEnumerable<MealLog>> GetMealLogsByDate(DateTime date, CancellationToken cancellationToken)
        {
            return await _context.MealLogs
                .Include(m => m.ProductQuantity)
                .ThenInclude(pq => pq.Product)
                .Where(m => m.Date.Date == date.Date)
                .ToListAsync(cancellationToken);
        }

        public async Task<MealLog> AddMealLog(MealLog mealLog, CancellationToken cancellationToken)
        {
            foreach (var productQuantity in mealLog.ProductQuantity)
            {
                var existingProduct = await GetProductById(productQuantity.Product.Id, cancellationToken);
                if (existingProduct != null)
                {
                    productQuantity.Product = existingProduct;
                }
                else
                {
                    throw new InvalidOperationException("Product not found");
                }
            }   
            _context.MealLogs.Add(mealLog);
            await _context.SaveChangesAsync(cancellationToken);
            return mealLog;
        }

        public async Task<MealLog> UpdateMealLog(MealLog MealLog, CancellationToken cancellationToken)
        {
            _context.MealLogs.Update(MealLog);
            await _context.SaveChangesAsync();
            return MealLog;
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
                .Include(p => p.Nutriments)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Product> GetProductByExternalId(string id, CancellationToken cancellationToken)
        {
            return await _context.Products
                .Include(p => p.Nutriments)
                .FirstOrDefaultAsync(p => p.IdExternal == id);
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

        public async Task<IEnumerable<SymptomsLog>> GetSymptomsLogsByDate(DateTime date, CancellationToken cancellationToken)
        {
            return await _context.SymptomsLogs
                .Where(s => s.Date.Date == date.Date)
                .ToListAsync(cancellationToken);
        }
    }
}

