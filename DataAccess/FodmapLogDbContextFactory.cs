using Azure.Identity;
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
            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "..", "FodmapLog.Server"))
                .AddJsonFile("appsettings.json", optional: true)
                .AddUserSecrets("7c0e8527-3921-4c01-a78c-c38c00c95cfd");

            var keyVaultName = configurationBuilder.Build()["KeyVaultName"];
            if (!string.IsNullOrWhiteSpace(keyVaultName))
            {
                configurationBuilder.AddAzureKeyVault(
                    new Uri($"https://{keyVaultName}.vault.azure.net/"),
                    new DefaultAzureCredential());
            }

            var configuration = configurationBuilder.Build();

            var optionsBuilder = new DbContextOptionsBuilder<FodmapLogDbContext>();
            optionsBuilder.UseSqlServer(configuration.GetConnectionString("prodConnection"));

            return new FodmapLogDbContext(optionsBuilder.Options);
        }
    }
}
