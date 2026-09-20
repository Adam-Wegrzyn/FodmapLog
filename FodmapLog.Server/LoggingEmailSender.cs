using Microsoft.AspNetCore.Identity.UI.Services;

namespace FodmapLog.Server
{
    /// <summary>
    /// Dev / interim email sender: logs reset links instead of sending mail.
    /// Replace with ACS/SendGrid for production mail delivery.
    /// </summary>
    public class LoggingEmailSender : IEmailSender
    {
        private readonly ILogger<LoggingEmailSender> _logger;

        public LoggingEmailSender(ILogger<LoggingEmailSender> logger)
        {
            _logger = logger;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            _logger.LogInformation(
                "Identity email (not sent). To={Email} Subject={Subject} Body={Body}",
                email,
                subject,
                htmlMessage);
            return Task.CompletedTask;
        }
    }
}
