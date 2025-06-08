using DataAccess.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class FodmapLogDbContext: IdentityDbContext
    {
        public FodmapLogDbContext(DbContextOptions<FodmapLogDbContext> options) : base(options)
        {
        }
   

        public DbSet<MealLog> MealLogs { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<SymptomsLog> SymptomsLogs { get; set; }
        public DbSet<Symptom> Symptoms { get; set; }
        public DbSet<SymptomType> SymptomTypes { get; set; }
        public DbSet<Unit> Units { get; set; }
        public DbSet<ProductQuantity> ProductQuantities { get; set; }
    }
}
