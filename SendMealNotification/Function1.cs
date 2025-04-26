using System;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SendMealNotification
{
    public class Function1
    {
        private readonly ILogger<Function1> _logger;
        private readonly IConfiguration _configuration;

        public Function1(ILogger<Function1> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [Function(nameof(Function1))]
        public void Run([ServiceBusTrigger("sample-topic", "sample-subscription", Connection = "serviceBusSecret2")] ServiceBusReceivedMessage message)
        {
            var connectionString = _configuration["serviceBusSecret2"];
            _logger.LogInformation($"Service Bus Connection String: {connectionString}");
            _logger.LogInformation("Message ID: {id}", message.MessageId);
            _logger.LogInformation("Message Body: {body}", message.Body);
            _logger.LogInformation("Message Content-Type: {contentType}", message.ContentType);
        }
    }
}