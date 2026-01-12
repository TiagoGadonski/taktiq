namespace GymHero.Application.Common.Interfaces;

/// <summary>
/// Service for sending messages via WhatsApp
/// </summary>
public interface IWhatsAppService
{
    /// <summary>
    /// Send a text message via WhatsApp
    /// </summary>
    Task<WhatsAppMessageResult> SendTextMessageAsync(
        string toPhoneNumber,
        string message,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Send a message with media (image, video, document) via WhatsApp
    /// </summary>
    Task<WhatsAppMessageResult> SendMediaMessageAsync(
        string toPhoneNumber,
        string mediaUrl,
        string? caption = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Send a workout plan via WhatsApp
    /// </summary>
    Task<WhatsAppMessageResult> SendWorkoutPlanAsync(
        string toPhoneNumber,
        string studentName,
        string workoutPlanName,
        string workoutPlanUrl,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Send a workout reminder via WhatsApp
    /// </summary>
    Task<WhatsAppMessageResult> SendWorkoutReminderAsync(
        string toPhoneNumber,
        string studentName,
        string workoutName,
        DateTime scheduledTime,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Send progress update via WhatsApp
    /// </summary>
    Task<WhatsAppMessageResult> SendProgressUpdateAsync(
        string toPhoneNumber,
        string studentName,
        string progressSummary,
        string? progressPhotoUrl = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Send assessment results via WhatsApp
    /// </summary>
    Task<WhatsAppMessageResult> SendAssessmentResultsAsync(
        string toPhoneNumber,
        string studentName,
        string assessmentSummary,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate if a phone number is WhatsApp enabled
    /// </summary>
    Task<bool> IsWhatsAppEnabledAsync(
        string phoneNumber,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of a WhatsApp message send operation
/// </summary>
public record WhatsAppMessageResult(
    bool Success,
    string? MessageId,
    string? ErrorMessage,
    DateTime SentAt
);
