using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;

var host = new HostBuilder()
    .ConfigureAppConfiguration((config) =>
    {
        config.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
              .AddEnvironmentVariables();
        var buildConfig = config.Build();
        var keyVaultName = buildConfig["KeyVaultName"];
        var tenantId = buildConfig["AzureAd:TenantId"];
        var clientId = buildConfig["AzureAd:ClientId"];
        var clientSecret = buildConfig["AzureAd:ClientSecret"];

        var clientSecretCredential = new ClientSecretCredential(tenantId, clientId, clientSecret);

        if (!string.IsNullOrEmpty(keyVaultName))
        {
            var keyVaultUri = new Uri($"https://{keyVaultName}.vault.azure.net/");
            config.AddAzureKeyVault(keyVaultUri, clientSecretCredential);
            var secretClient = new SecretClient(keyVaultUri, clientSecretCredential);
            var serviceBusConnectionString = GetServiceBusConnectionString(secretClient, "serviceBusSecret2");
            buildConfig["Values:serviceBusSecret2"] = serviceBusConnectionString;
            Console.WriteLine($"Retrieved Service Bus Connection String: {serviceBusConnectionString}");
        }
    })
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureLogging(logging =>
    {
        logging.AddConsole();
        logging.SetMinimumLevel(LogLevel.Debug);
    })
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
    })
    .Build();

host.Run();

static string GetServiceBusConnectionString(SecretClient secretClient, string secretName)
{
    try
    {
        KeyVaultSecret secret = secretClient.GetSecret(secretName);
        return secret.Value;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Failed to retrieve secret: {ex.Message}");
        throw;
    }
}