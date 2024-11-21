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
        public async Task<IEnumerable<MealLog>> GetAllMealLogs()
        {
            return await _context.Set<MealLog>().ToListAsync();
        }

        public async Task<MealLog> GetMealLog(int id)
        {
            return await _context.MealLogs.FindAsync(id);
        }

        public async Task<MealLog> AddMealLog(MealLog mealLog)
        {
            _context.MealLogs.Add(mealLog);
            await _context.SaveChangesAsync();
            return mealLog;
        }

        public async Task<MealLog> UpdateMealLog(MealLog mealLog)
        {
            _context.MealLogs.Update(mealLog);
            await _context.SaveChangesAsync();
            return mealLog;
        }

        public async Task<MealLog> DeleteMealLog(int id)
        {
            var mealLog = await _context.MealLogs.FindAsync(id);
            if (mealLog != null)
            {
                _context.MealLogs.Remove(mealLog);
                await _context.SaveChangesAsync();
            }
            return mealLog;
        }

        // Methods for Product
        public async Task<IEnumerable<Product>> GetAllProducts()
        {
            return await _context.Products.ToListAsync();
        }

        public async Task<Product> GetProduct(int id)
        {
            return await _context.Products.FindAsync(id);
        }

        public async Task<Product> AddProduct(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<Product> UpdateProduct(Product product)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<Product> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
            return product;
        }
    }
}

