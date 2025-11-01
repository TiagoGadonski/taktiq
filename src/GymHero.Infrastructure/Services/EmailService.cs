using GymHero.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace GymHero.Infrastructure.Services;

/// <summary>
/// Email service implementation.
/// For development: logs emails to console.
/// For production: can be replaced with SendGrid, SMTP, or other email providers.
/// </summary>
public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public Task SendPasswordResetEmailAsync(string email, string resetToken)
    {
        // For development: log to console
        // For production: replace with actual email sending (SendGrid, SMTP, etc.)

        _logger.LogInformation(
            "========================================\n" +
            "PASSWORD RESET EMAIL\n" +
            "To: {Email}\n" +
            "Reset Token: {ResetToken}\n" +
            "========================================",
            email, resetToken);

        // In production, you would send an actual email here
        // Example with SendGrid:
        // await _sendGridClient.SendEmailAsync(from, to, subject, plainTextContent, htmlContent);

        return Task.CompletedTask;
    }

    public Task SendWelcomeEmailAsync(string email, string userName)
    {
        _logger.LogInformation(
            "========================================\n" +
            "WELCOME EMAIL\n" +
            "To: {Email}\n" +
            "User: {UserName}\n" +
            "Welcome to GymHero!\n" +
            "========================================",
            email, userName);

        return Task.CompletedTask;
    }
}
