using GymHero.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace GymHero.Infrastructure.Services;

/// <summary>
/// Email service implementation using SendGrid.
/// </summary>
public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly ISendGridClient _sendGridClient;
    private readonly string _fromEmail;
    private readonly string _fromName;

    public EmailService(
        ILogger<EmailService> logger,
        ISendGridClient sendGridClient)
    {
        _logger = logger;
        _sendGridClient = sendGridClient;
        _fromEmail = "noreply@taktiq.app";
        _fromName = "TaktIQ";

        // The actual API key validation is done in DependencyInjection.cs
        // This service will still work, but emails won't be sent with invalid/placeholder keys
        _logger.LogInformation("EmailService initialized. Emails will be sent from {FromEmail}", _fromEmail);
    }

    public async Task SendPasswordResetEmailAsync(string email, string resetToken)
    {
        try
        {
            var resetUrl = $"https://taktiq.app/reset-password?token={resetToken}";

            var from = new EmailAddress(_fromEmail, _fromName);
            var to = new EmailAddress(email);
            var subject = "Redefinição de Senha - TaktIQ";

            var plainTextContent = $"Clique no link para redefinir sua senha: {resetUrl}";
            var htmlContent = $@"
<!DOCTYPE html>
<html>
<body>
    <h2>Redefinição de Senha</h2>
    <p>Clique no botão abaixo para redefinir sua senha:</p>
    <a href=""{resetUrl}"" style=""display: inline-block; background: #667eea; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px;"">Redefinir Senha</a>
    <p>Ou copie e cole este link no seu navegador: {resetUrl}</p>
    <p>Este link expira em 1 hora.</p>
</body>
</html>";

            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await _sendGridClient.SendEmailAsync(msg);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Password reset email sent successfully to {Email}", email);
            }
            else
            {
                var body = await response.Body.ReadAsStringAsync();
                _logger.LogError("Failed to send password reset email to {Email}. Status: {Status}, Response: {Response}",
                    email, response.StatusCode, body);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending password reset email to {Email}", email);
            throw;
        }
    }

    public async Task SendWelcomeEmailAsync(string email, string userName)
    {
        try
        {
            var from = new EmailAddress(_fromEmail, _fromName);
            var to = new EmailAddress(email);
            var subject = "Bem-vindo ao TaktIQ! 🎯";

            var plainTextContent = $"Bem-vindo ao TaktIQ, {userName}! Estamos felizes em tê-lo conosco.";
            var htmlContent = $@"
<!DOCTYPE html>
<html>
<body style=""font-family: Arial, sans-serif; line-height: 1.6; color: #333;"">
    <h2>Bem-vindo ao TaktIQ, {userName}! 🎯</h2>
    <p>Estamos muito felizes em tê-lo conosco!</p>
    <p>Comece sua jornada de transformação agora mesmo.</p>
    <a href=""https://taktiq.app"" style=""display: inline-block; background: #667eea; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; margin-top: 10px;"">Acessar TaktIQ</a>
</body>
</html>";

            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await _sendGridClient.SendEmailAsync(msg);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Welcome email sent successfully to {Email}", email);
            }
            else
            {
                var body = await response.Body.ReadAsStringAsync();
                _logger.LogError("Failed to send welcome email to {Email}. Status: {Status}, Response: {Response}",
                    email, response.StatusCode, body);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending welcome email to {Email}", email);
            throw;
        }
    }

    public async Task SendStudentInvitationEmailAsync(string email, string trainerName, string activationToken, string workoutPlanName)
    {
        var activationUrl = $"https://taktiq.app/activate?token={activationToken}";

        var htmlContent = $@"
<!DOCTYPE html>
<html lang=""pt-BR"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Convite do Personal Trainer</title>
</head>
<body style=""margin: 0; padding: 0; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); min-height: 100vh;"">
    <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""max-width: 600px; margin: 0 auto; padding: 40px 20px;"">
        <tr>
            <td align=""center"">
                <!-- Logo -->
                <div style=""background: rgba(255, 255, 255, 0.1); backdrop-filter: blur(10px); border-radius: 20px; padding: 30px; margin-bottom: 30px; border: 1px solid rgba(255, 255, 255, 0.2);"">
                    <h1 style=""color: #ffffff; font-size: 48px; margin: 0; font-weight: 800; text-shadow: 0 2px 4px rgba(0,0,0,0.2);"">
                        TAKT<span style=""color: #fbbf24;"">IQ</span>
                    </h1>
                    <p style=""color: rgba(255, 255, 255, 0.9); margin: 10px 0 0 0; font-size: 14px; letter-spacing: 2px;"">TRAIN SMARTER</p>
                </div>

                <!-- Main Card -->
                <div style=""background: #ffffff; border-radius: 20px; padding: 40px; box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3); text-align: left;"">
                    <!-- Header Icon -->
                    <div style=""text-align: center; margin-bottom: 30px;"">
                        <div style=""display: inline-block; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); width: 80px; height: 80px; border-radius: 50%; padding: 20px; box-shadow: 0 10px 25px rgba(102, 126, 234, 0.3);"">
                            <svg xmlns=""http://www.w3.org/2000/svg"" width=""40"" height=""40"" viewBox=""0 0 24 24"" fill=""none"" stroke=""#ffffff"" stroke-width=""2"" stroke-linecap=""round"" stroke-linejoin=""round"">
                                <path d=""M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2""></path>
                                <circle cx=""9"" cy=""7"" r=""4""></circle>
                                <path d=""M22 21v-2a4 4 0 0 0-3-3.87""></path>
                                <path d=""M16 3.13a4 4 0 0 1 0 7.75""></path>
                            </svg>
                        </div>
                    </div>

                    <h2 style=""color: #1e293b; font-size: 28px; margin: 0 0 20px 0; font-weight: 700; text-align: center;"">
                        Você foi Convidado! 🎯
                    </h2>

                    <p style=""color: #475569; font-size: 16px; line-height: 1.6; margin: 0 0 25px 0;"">
                        Olá! 👋
                    </p>

                    <p style=""color: #475569; font-size: 16px; line-height: 1.6; margin: 0 0 25px 0;"">
                        <strong style=""color: #667eea;"">{trainerName}</strong> te convidou para participar da plataforma <strong>TaktIQ</strong> e criou um plano de treino especial para você:
                    </p>

                    <!-- Workout Plan Card -->
                    <div style=""background: linear-gradient(135deg, #f0f4ff 0%, #e8edff 100%); border-left: 4px solid #667eea; padding: 20px; border-radius: 12px; margin: 25px 0;"">
                        <div style=""display: flex; align-items: center; gap: 10px;"">
                            <svg xmlns=""http://www.w3.org/2000/svg"" width=""24"" height=""24"" viewBox=""0 0 24 24"" fill=""none"" stroke=""#667eea"" stroke-width=""2"" stroke-linecap=""round"" stroke-linejoin=""round"">
                                <path d=""M6.5 6.5l11 11""></path>
                                <path d=""M21 21l-1-1""></path>
                                <path d=""M3 21l9-9""></path>
                                <path d=""M3 3l1 1""></path>
                                <path d=""M21 3l-9 9""></path>
                            </svg>
                            <div>
                                <p style=""margin: 0; color: #64748b; font-size: 12px; text-transform: uppercase; letter-spacing: 1px; font-weight: 600;"">Plano de Treino</p>
                                <p style=""margin: 5px 0 0 0; color: #1e293b; font-size: 18px; font-weight: 700;"">{workoutPlanName}</p>
                            </div>
                        </div>
                    </div>

                    <p style=""color: #475569; font-size: 16px; line-height: 1.6; margin: 0 0 30px 0;"">
                        Para começar sua jornada de transformação, clique no botão abaixo para ativar sua conta:
                    </p>

                    <!-- CTA Button -->
                    <div style=""text-align: center; margin: 30px 0;"">
                        <a href=""{activationUrl}"" style=""display: inline-block; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: #ffffff; text-decoration: none; padding: 16px 40px; border-radius: 50px; font-weight: 700; font-size: 16px; box-shadow: 0 10px 25px rgba(102, 126, 234, 0.4); transition: all 0.3s ease;"">
                            ✨ Ativar Minha Conta
                        </a>
                    </div>

                    <!-- Info Box -->
                    <div style=""background: #fef3c7; border-left: 4px solid #fbbf24; padding: 15px; border-radius: 8px; margin: 25px 0;"">
                        <p style=""margin: 0; color: #92400e; font-size: 14px; line-height: 1.5;"">
                            <strong>💡 Dica:</strong> Após ativar sua conta, você poderá completar seu perfil, visualizar seus treinos personalizados e acompanhar seu progresso!
                        </p>
                    </div>

                    <!-- Alternative Link -->
                    <p style=""color: #94a3b8; font-size: 13px; line-height: 1.6; margin: 25px 0 0 0; text-align: center;"">
                        Se o botão não funcionar, copie e cole este link no seu navegador:<br>
                        <a href=""{activationUrl}"" style=""color: #667eea; word-break: break-all;"">{activationUrl}</a>
                    </p>
                </div>

                <!-- Footer -->
                <div style=""margin-top: 30px; text-align: center;"">
                    <p style=""color: rgba(255, 255, 255, 0.8); font-size: 14px; margin: 0 0 10px 0;"">
                        Este convite expira em 7 dias
                    </p>
                    <p style=""color: rgba(255, 255, 255, 0.6); font-size: 12px; margin: 0;"">
                        © 2024 TaktIQ. Todos os direitos reservados.
                    </p>
                    <div style=""margin-top: 15px;"">
                        <a href=""https://taktiq.app"" style=""color: rgba(255, 255, 255, 0.9); text-decoration: none; font-size: 12px; margin: 0 10px;"">Site</a>
                        <span style=""color: rgba(255, 255, 255, 0.4);"">•</span>
                        <a href=""https://taktiq.app/about"" style=""color: rgba(255, 255, 255, 0.9); text-decoration: none; font-size: 12px; margin: 0 10px;"">Sobre</a>
                        <span style=""color: rgba(255, 255, 255, 0.4);"">•</span>
                        <a href=""https://taktiq.app/privacy"" style=""color: rgba(255, 255, 255, 0.9); text-decoration: none; font-size: 12px; margin: 0 10px;"">Privacidade</a>
                    </div>
                </div>
            </td>
        </tr>
    </table>
</body>
</html>";

        try
        {
            var from = new EmailAddress(_fromEmail, _fromName);
            var to = new EmailAddress(email);
            var subject = $"{trainerName} te convidou para o TaktIQ! 🎯";

            var plainTextContent = $"Você foi convidado por {trainerName} para o TaktIQ. Acesse {activationUrl} para ativar sua conta.";

            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await _sendGridClient.SendEmailAsync(msg);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation(
                    "Student invitation email sent successfully. To: {Email}, Trainer: {TrainerName}, Plan: {WorkoutPlanName}",
                    email, trainerName, workoutPlanName);
            }
            else
            {
                var body = await response.Body.ReadAsStringAsync();
                _logger.LogError(
                    "Failed to send student invitation email. To: {Email}, Status: {Status}, Response: {Response}",
                    email, response.StatusCode, body);
                throw new Exception($"Failed to send email. Status: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending student invitation email to {Email}", email);
            throw;
        }
    }
}
