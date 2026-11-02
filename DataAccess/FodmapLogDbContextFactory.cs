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
            var cwd = Directory.GetCurrentDirectory();
            var serverPath = Path.Combine(cwd, "..", "FodmapLog.Server");
            if (!Directory.Exists(serverPath))
            {
                serverPath = Path.Combine(cwd, "FodmapLog.Server");
            }

            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(serverPath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddUserSecrets("7c0e8527-3921-4c01-a78c-c38c00c95cfd")
                .AddEnvironmentVariables();

            var configuration = configurationBuilder.Build();
            var connection = configuration.GetConnectionString("prodConnection");

            // CI passes --connection / env; do not require Key Vault access for that path.
            if (string.IsNullOrWhiteSpace(connection))
            {
                var keyVaultName = configuration["KeyVaultName"];
                if (!string.IsNullOrWhiteSpace(keyVaultName))
                {
                    try
                    {
                        configurationBuilder.AddAzureKeyVault(
                            new Uri($"https://{keyVaultName}.vault.azure.net/"),
                            new DefaultAzureCredential());
                        configuration = configurationBuilder.Build();
                        connection = configuration.GetConnectionString("prodConnection");
                    }
                    catch
                    {
                        // Caller may still pass --connection to dotnet ef.
                    }
                }
            }

            var optionsBuilder = new DbContextOptionsBuilder<FodmapLogDbContext>();
            optionsBuilder.UseSqlServer(
                connection ?? "Server=(localdb)\\mssqllocaldb;Database=FodmapLog;Trusted_Connection=True;",
                sql => sql.EnableRetryOnFailure(10, TimeSpan.FromSeconds(30), null));

            return new FodmapLogDbContext(optionsBuilder.Options);
        }
    }
}
