using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace DataAccess
{
    public class FodmapLogDbContextFactory : IDesignTimeDbContextFactory<FodmapLogDbContext>
    {
        public FodmapLogDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "..", "FodmapLog.Server"))
                .AddJsonFile("appsettings.json", optional: true)
                .AddUserSecrets("7c0e8527-3921-4c01-a78c-c38c00c95cfd") // <-- This line loads User Secrets
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<FodmapLogDbContext>();
            optionsBuilder.UseSqlServer(configuration.GetConnectionString("devConnectionAzure"));

            return new FodmapLogDbContext(optionsBuilder.Options);
        }
    }
}