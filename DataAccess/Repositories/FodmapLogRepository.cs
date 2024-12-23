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

        // Methods for Meal
        public async Task<IEnumerable<Meal>> GetAllMeals()
        {
            return await _context.Set<Meal>().ToListAsync();
        }

        public async Task<Meal> GetMeal(int id)
        {
            return await _context.Meals.FindAsync(id);
        }

        public async Task<Meal> AddMeal(Meal Meal)
        {
            _context.Meals.Add(Meal);
            await _context.SaveChangesAsync();
            return Meal;
        }

        public async Task<Meal> UpdateMeal(Meal Meal)
        {
            _context.Meals.Update(Meal);
            await _context.SaveChangesAsync();
            return Meal;
        }

        public async Task<Meal> DeleteMeal(int id)
        {
            var Meal = await _context.Meals.FindAsync(id);
            if (Meal != null)
            {
                _context.Meals.Remove(Meal);
                await _context.SaveChangesAsync();
            }
            return Meal;
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

