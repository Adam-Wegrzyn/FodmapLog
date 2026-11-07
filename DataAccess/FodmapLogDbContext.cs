using DataAccess.Entities;
using DataAccess.Enums;
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

            modelBuilder.Entity<MealLog>(entity =>
            {
                entity.Property(e => e.UserId).HasMaxLength(450);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => new { e.UserId, e.Date });
            });

            modelBuilder.Entity<SymptomsLog>(entity =>
            {
                entity.Property(e => e.UserId).HasMaxLength(450);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => new { e.UserId, e.Date });
            });

            modelBuilder.Entity<SymptomType>(entity =>
            {
                entity.Property(e => e.Name).HasMaxLength(200);
                entity.Property(e => e.UserId).HasMaxLength(450);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => new { e.UserId, e.Name });
            });

            modelBuilder.Entity<SymptomType>().HasData(
                Seed(1, "Nausea"),
                Seed(2, "Burping"),
                Seed(3, "Diarrhea"),
                Seed(4, "Constipation"),
                Seed(5, "Bloating"),
                Seed(6, "Abdominal Pain"),
                Seed(7, "Heartburn"),
                Seed(8, "Gas"),
                Seed(9, "Cramps"),
                Seed(10, "Vomiting"),
                Seed(11, "Appetite", SymptomScaleKind.Positive),
                Seed(12, "Headache"),
                Seed(13, "Fatigue"),
                Seed(14, "Mood", SymptomScaleKind.Positive),
                Seed(15, "Energy", SymptomScaleKind.Positive),
                Seed(16, "Sleep Quality", SymptomScaleKind.Positive),
                Seed(17, "Stress"),
                Seed(18, "Concentration", SymptomScaleKind.Positive),
                Seed(19, "Motivation", SymptomScaleKind.Positive),
                Seed(20, "Physical Activity", SymptomScaleKind.Positive),
                Seed(21, "General Well-being", SymptomScaleKind.Positive),
                Seed(22, "Brain Fog"),
                Seed(23, "Dizziness"),
                Seed(24, "Anxiety"),
                Seed(25, "Joint Pain"),
                Seed(26, "Itching"),
                Seed(27, "Urgency"),
                Seed(28, "Incomplete Evacuation"),
                Seed(29, "Mucus"),
                Seed(30, "Palpitations"),
                Seed(31, "Irritability")
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

        private static SymptomType Seed(int id, string name, SymptomScaleKind kind = SymptomScaleKind.Negative)
            => new() { Id = id, Name = name, ScaleKind = kind };
    }
}
