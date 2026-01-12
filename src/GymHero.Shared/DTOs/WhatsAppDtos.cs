namespace GymHero.Shared.DTOs;

/// <summary>
/// Request to send a text message via WhatsApp
/// </summary>
public record SendTextMessageRequest(
    string ToPhoneNumber,
    string Message
);

/// <summary>
/// Request to send media (image/video/document) via WhatsApp
/// </summary>
public record SendMediaMessageRequest(
    string ToPhoneNumber,
    string MediaUrl,
    string? Caption = null
);

/// <summary>
/// Request to send workout plan notification via WhatsApp
/// </summary>
public record SendWorkoutPlanNotificationRequest(
    Guid StudentId,
    string WorkoutPlanName,
    string WorkoutPlanUrl
);

/// <summary>
/// Request to send workout reminder via WhatsApp
/// </summary>
public record SendWorkoutReminderRequest(
    Guid StudentId,
    string WorkoutName,
    DateTime ScheduledTime
);

/// <summary>
/// Request to send progress update via WhatsApp
/// </summary>
public record SendProgressUpdateRequest(
    Guid StudentId,
    string ProgressSummary,
    string? ProgressPhotoUrl = null
);

/// <summary>
/// Request to send assessment results via WhatsApp
/// </summary>
public record SendAssessmentResultsRequest(
    Guid StudentId,
    string AssessmentSummary
);

/// <summary>
/// Response from WhatsApp message send operation
/// </summary>
public record WhatsAppMessageResponse(
    bool Success,
    string? MessageId,
    string? ErrorMessage,
    DateTime SentAt
);
