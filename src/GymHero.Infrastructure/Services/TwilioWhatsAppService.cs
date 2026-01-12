using GymHero.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace GymHero.Infrastructure.Services;

/// <summary>
/// WhatsApp messaging service using Twilio API
/// </summary>
public class TwilioWhatsAppService : IWhatsAppService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TwilioWhatsAppService> _logger;
    private readonly string? _accountSid;
    private readonly string? _authToken;
    private readonly string? _fromNumber;
    private readonly bool _isEnabled;

    public TwilioWhatsAppService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<TwilioWhatsAppService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;

        _accountSid = configuration["Twilio:AccountSid"];
        _authToken = configuration["Twilio:AuthToken"];
        _fromNumber = configuration["Twilio:WhatsAppNumber"];
        _isEnabled = configuration.GetValue<bool>("Twilio:WhatsApp:Enabled");

        if (_isEnabled && (!string.IsNullOrEmpty(_accountSid) && !string.IsNullOrEmpty(_authToken)))
        {
            var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_accountSid}:{_authToken}"));
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);
        }
    }

    public async Task<WhatsAppMessageResult> SendTextMessageAsync(
        string toPhoneNumber,
        string message,
        CancellationToken cancellationToken = default)
    {
        if (!_isEnabled)
        {
            _logger.LogWarning("WhatsApp service is disabled. Message not sent to {PhoneNumber}", toPhoneNumber);
            return new WhatsAppMessageResult(false, null, "WhatsApp service is disabled", DateTime.UtcNow);
        }

        if (string.IsNullOrEmpty(_accountSid) || string.IsNullOrEmpty(_authToken) || string.IsNullOrEmpty(_fromNumber))
        {
            _logger.LogError("Twilio WhatsApp credentials are not configured");
            return new WhatsAppMessageResult(false, null, "WhatsApp service not configured", DateTime.UtcNow);
        }

        try
        {
            var formattedTo = FormatPhoneNumber(toPhoneNumber);
            var formattedFrom = FormatPhoneNumber(_fromNumber);

            var url = $"https://api.twilio.com/2010-04-01/Accounts/{_accountSid}/Messages.json";

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("From", formattedFrom),
                new KeyValuePair<string, string>("To", formattedTo),
                new KeyValuePair<string, string>("Body", message)
            });

            var response = await _httpClient.PostAsync(url, content, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                var jsonDoc = JsonDocument.Parse(responseContent);
                var messageId = jsonDoc.RootElement.GetProperty("sid").GetString();

                _logger.LogInformation("WhatsApp message sent successfully to {PhoneNumber}, MessageId: {MessageId}",
                    toPhoneNumber, messageId);

                return new WhatsAppMessageResult(true, messageId, null, DateTime.UtcNow);
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to send WhatsApp message to {PhoneNumber}. Status: {Status}, Error: {Error}",
                    toPhoneNumber, response.StatusCode, errorContent);

                return new WhatsAppMessageResult(false, null, $"Failed to send: {response.StatusCode}", DateTime.UtcNow);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while sending WhatsApp message to {PhoneNumber}", toPhoneNumber);
            return new WhatsAppMessageResult(false, null, ex.Message, DateTime.UtcNow);
        }
    }

    public async Task<WhatsAppMessageResult> SendMediaMessageAsync(
        string toPhoneNumber,
        string mediaUrl,
        string? caption = null,
        CancellationToken cancellationToken = default)
    {
        if (!_isEnabled)
        {
            _logger.LogWarning("WhatsApp service is disabled. Media message not sent to {PhoneNumber}", toPhoneNumber);
            return new WhatsAppMessageResult(false, null, "WhatsApp service is disabled", DateTime.UtcNow);
        }

        if (string.IsNullOrEmpty(_accountSid) || string.IsNullOrEmpty(_authToken) || string.IsNullOrEmpty(_fromNumber))
        {
            _logger.LogError("Twilio WhatsApp credentials are not configured");
            return new WhatsAppMessageResult(false, null, "WhatsApp service not configured", DateTime.UtcNow);
        }

        try
        {
            var formattedTo = FormatPhoneNumber(toPhoneNumber);
            var formattedFrom = FormatPhoneNumber(_fromNumber);

            var url = $"https://api.twilio.com/2010-04-01/Accounts/{_accountSid}/Messages.json";

            var formData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("From", formattedFrom),
                new KeyValuePair<string, string>("To", formattedTo),
                new KeyValuePair<string, string>("MediaUrl", mediaUrl)
            };

            if (!string.IsNullOrEmpty(caption))
            {
                formData.Add(new KeyValuePair<string, string>("Body", caption));
            }

            var content = new FormUrlEncodedContent(formData);

            var response = await _httpClient.PostAsync(url, content, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                var jsonDoc = JsonDocument.Parse(responseContent);
                var messageId = jsonDoc.RootElement.GetProperty("sid").GetString();

                _logger.LogInformation("WhatsApp media message sent successfully to {PhoneNumber}, MessageId: {MessageId}",
                    toPhoneNumber, messageId);

                return new WhatsAppMessageResult(true, messageId, null, DateTime.UtcNow);
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to send WhatsApp media message to {PhoneNumber}. Status: {Status}, Error: {Error}",
                    toPhoneNumber, response.StatusCode, errorContent);

                return new WhatsAppMessageResult(false, null, $"Failed to send: {response.StatusCode}", DateTime.UtcNow);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while sending WhatsApp media message to {PhoneNumber}", toPhoneNumber);
            return new WhatsAppMessageResult(false, null, ex.Message, DateTime.UtcNow);
        }
    }

    public async Task<WhatsAppMessageResult> SendWorkoutPlanAsync(
        string toPhoneNumber,
        string studentName,
        string workoutPlanName,
        string workoutPlanUrl,
        CancellationToken cancellationToken = default)
    {
        var message = $"""
            🏋️ Olá {studentName}!

            Seu novo plano de treino está pronto: *{workoutPlanName}*

            Acesse aqui: {workoutPlanUrl}

            Bons treinos! 💪
            """;

        return await SendTextMessageAsync(toPhoneNumber, message, cancellationToken);
    }

    public async Task<WhatsAppMessageResult> SendWorkoutReminderAsync(
        string toPhoneNumber,
        string studentName,
        string workoutName,
        DateTime scheduledTime,
        CancellationToken cancellationToken = default)
    {
        var timeStr = scheduledTime.ToString("HH:mm");
        var message = $"""
            ⏰ Lembrete de Treino!

            Olá {studentName}!

            Está na hora do seu treino: *{workoutName}*
            Horário: {timeStr}

            Vamos lá! 💪🔥
            """;

        return await SendTextMessageAsync(toPhoneNumber, message, cancellationToken);
    }

    public async Task<WhatsAppMessageResult> SendProgressUpdateAsync(
        string toPhoneNumber,
        string studentName,
        string progressSummary,
        string? progressPhotoUrl = null,
        CancellationToken cancellationToken = default)
    {
        var message = $"""
            📊 Atualização de Progresso

            Olá {studentName}!

            {progressSummary}

            Continue assim! 🎯✨
            """;

        if (!string.IsNullOrEmpty(progressPhotoUrl))
        {
            return await SendMediaMessageAsync(toPhoneNumber, progressPhotoUrl, message, cancellationToken);
        }

        return await SendTextMessageAsync(toPhoneNumber, message, cancellationToken);
    }

    public async Task<WhatsAppMessageResult> SendAssessmentResultsAsync(
        string toPhoneNumber,
        string studentName,
        string assessmentSummary,
        CancellationToken cancellationToken = default)
    {
        var message = $"""
            📋 Resultados da Avaliação

            Olá {studentName}!

            {assessmentSummary}

            Estamos juntos nessa jornada! 💪
            """;

        return await SendTextMessageAsync(toPhoneNumber, message, cancellationToken);
    }

    public async Task<bool> IsWhatsAppEnabledAsync(
        string phoneNumber,
        CancellationToken cancellationToken = default)
    {
        // Note: Twilio doesn't provide a direct way to check if a number has WhatsApp
        // This would require the WhatsApp Business API which has different requirements
        // For now, we'll return true if the service is enabled
        await Task.CompletedTask;
        return _isEnabled;
    }

    private string FormatPhoneNumber(string phoneNumber)
    {
        // Ensure the number has the whatsapp: prefix for Twilio
        if (!phoneNumber.StartsWith("whatsapp:"))
        {
            return $"whatsapp:{phoneNumber}";
        }
        return phoneNumber;
    }
}
