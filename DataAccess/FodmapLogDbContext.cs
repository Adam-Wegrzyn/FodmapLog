﻿using DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class FodmapLogDbContext: DbContext
    {
        public FodmapLogDbContext(DbContextOptions<FodmapLogDbContext> options) : base(options)
        {
        }

        public DbSet<Meal> Meals { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<SymptomsLog> SymptomsLogs { get; set; }

        
       
    }
}
