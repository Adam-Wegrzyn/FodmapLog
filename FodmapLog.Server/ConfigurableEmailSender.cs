using System.Net;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace FodmapLog.Server
{
    /// <summary>
    /// Sends Identity mail (password reset) via SendGrid or SMTP.
    /// Does not log message bodies (they contain reset codes).
    /// </summary>
    public class ConfigurableEmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ConfigurableEmailSender> _logger;

        public ConfigurableEmailSender(
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            ILogger<ConfigurableEmailSender> logger)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var from = _configuration["Email:From"];
            var html = Wrap(htmlMessage);

            var sendGridKey = _configuration["Email:SendGridApiKey"];
            if (!string.IsNullOrWhiteSpace(sendGridKey))
            {
                await SendSendGridAsync(sendGridKey, from, email, subject, html);
                return;
            }

            var host = _configuration["Email:SmtpHost"];
            if (!string.IsNullOrWhiteSpace(host) && !string.IsNullOrWhiteSpace(from))
            {
                await SendSmtpAsync(host, from, email, subject, html);
                return;
            }

            _logger.LogError("Email is not configured. Set Email__SendGridApiKey or Email__SmtpHost + Email__From.");
            throw new InvalidOperationException("Email is not configured.");
        }

        private string Wrap(string identityHtml)
        {
            var appUrl = _configuration["Email:AppBaseUrl"]
                ?? "https://healthygultlog-app.azurewebsites.net";
            var resetUrl = appUrl.TrimEnd('/') + "/reset-password";
            return $"""
                <p>Mova</p>
                <p>Use this code on the Reset password page. It expires; if it does not work, request a new one.</p>
                <p><a href="{resetUrl}">{resetUrl}</a></p>
                <div>{identityHtml}</div>
                """;
        }

        private async Task SendSendGridAsync(string apiKey, string? from, string to, string subject, string html)
        {
            if (string.IsNullOrWhiteSpace(from))
            {
                throw new InvalidOperationException("Email__From is required with SendGrid.");
            }

            var payload = JsonSerializer.Serialize(new
            {
                personalizations = new[] { new { to = new[] { new { email = to } } } },
                from = new { email = from },
                subject,
                content = new[] { new { type = "text/html", value = html } }
            });

            using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.sendgrid.com/v3/mail/send");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            request.Content = new StringContent(payload, Encoding.UTF8, "application/json");

            var client = _httpClientFactory.CreateClient();
            using var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("SendGrid rejected the message. Status={Status}", (int)response.StatusCode);
                throw new InvalidOperationException("Failed to send email.");
            }

            _logger.LogInformation("Password reset email sent via SendGrid.");
        }

        private async Task SendSmtpAsync(string host, string from, string to, string subject, string html)
        {
            var port = _configuration.GetValue("Email:SmtpPort", 587);
            var user = _configuration["Email:SmtpUser"];
            var password = _configuration["Email:SmtpPassword"];

            using var message = new MailMessage();
            message.From = new MailAddress(from);
            message.To.Add(to);
            message.Subject = subject;
            message.Body = html;
            message.IsBodyHtml = true;

            using var client = new SmtpClient(host, port)
            {
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network
            };
            if (!string.IsNullOrWhiteSpace(user))
            {
                client.Credentials = new NetworkCredential(user, password);
            }

            await client.SendMailAsync(message);
            _logger.LogInformation("Password reset email sent via SMTP.");
        }
    }
}
