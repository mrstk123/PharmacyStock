using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;
using PharmacyStock.Application.Interfaces;

namespace PharmacyStock.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly IConfiguration _configuration;

    public EmailService(ILogger<EmailService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<bool> SendEmailAsync(string to, string subject, string body)
    {
        var smtpServer = _configuration["EmailSettings:SmtpServer"];
        var portString = _configuration["EmailSettings:Port"];
        var senderEmail = _configuration["EmailSettings:SenderEmail"];
        var senderName = _configuration["EmailSettings:SenderName"];
        var username = _configuration["EmailSettings:Username"];
        var password = _configuration["EmailSettings:Password"];

        // Validate configuration
        if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(senderEmail))
        {
            _logger.LogError("Email configuration is missing. Cannot send email.");
            return false;
        }

        if (!int.TryParse(portString, out int port))
        {
            port = 587; // Default SMTP port
        }

        try
        {
            using var message = new MailMessage();
            message.From = new MailAddress(senderEmail, senderName ?? "Pharmacy Stock System");
            message.To.Add(new MailAddress(to));
            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = true;

            using var client = new SmtpClient(smtpServer, port);
            client.Credentials = new NetworkCredential(username, password);
            client.EnableSsl = true;

            await client.SendMailAsync(message);
            _logger.LogInformation("Email sent successfully to {To}", to);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}", to);
            return false;
        }
    }
}
