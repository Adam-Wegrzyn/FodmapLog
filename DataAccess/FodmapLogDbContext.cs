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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<SymptomType>().HasData(
                new SymptomType { Id = 1, Name = "Nausea" },
                new SymptomType { Id = 2, Name = "Burping" },
                new SymptomType { Id = 3, Name = "Diarrhea" },
                new SymptomType { Id = 4, Name = "Constipation" },
                new SymptomType { Id = 5, Name = "Bloating" },
                new SymptomType { Id = 6, Name = "Abdominal Pain" },
                new SymptomType { Id = 7, Name = "Heartburn" },
                new SymptomType { Id = 8, Name = "Gas" },
                new SymptomType { Id = 9, Name = "Cramps" },
                new SymptomType { Id = 10, Name = "Vomiting" },
                new SymptomType { Id = 11, Name = "Appetite" },
                new SymptomType { Id = 12, Name = "Headache" },
                new SymptomType { Id = 13, Name = "Fatigue" },
                new SymptomType { Id = 14, Name = "Mood" },
                new SymptomType { Id = 15, Name = "Energy" },
                new SymptomType { Id = 16, Name = "Sleep Quality" },
                new SymptomType { Id = 17, Name = "Stress" },
                new SymptomType { Id = 18, Name = "Concentration" },
                new SymptomType { Id = 19, Name = "Motivation" },
                new SymptomType { Id = 20, Name = "Physical Activity" },
                new SymptomType { Id = 21, Name = "General Well-being" }
            );

            modelBuilder.Entity<Unit>().HasData(
             new Unit { Id = 1, Name = "Gram" },
             new Unit { Id = 2, Name = "Kilogram" },
             new Unit { Id = 3, Name = "Milligram" },
             new Unit { Id = 4, Name = "Liter" },
             new Unit { Id = 5, Name = "Milliliter" },
             new Unit { Id = 6, Name = "Teaspoon" },
             new Unit { Id = 7, Name = "Tablespoon" },
             new Unit { Id = 8, Name = "Cup" },
             new Unit { Id = 9, Name = "Piece" },
             new Unit { Id = 10, Name = "Slice" },
             new Unit { Id = 11, Name = "Drop" },
             new Unit { Id = 12, Name = "Pinch" },
             new Unit { Id = 13, Name = "Ounce" },
             new Unit { Id = 14, Name = "Pound" },
             new Unit { Id = 15, Name = "Fluid Ounce" },
             new Unit { Id = 16, Name = "Pint" },
             new Unit { Id = 17, Name = "Quart" },
             new Unit { Id = 18, Name = "Gallon" },
             new Unit { Id = 19, Name = "Can" },
             new Unit { Id = 20, Name = "Package" },
             new Unit { Id = 21, Name = "Bottle" }
 );
        }
    }
}
