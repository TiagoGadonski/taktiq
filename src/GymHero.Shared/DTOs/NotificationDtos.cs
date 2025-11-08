namespace GymHero.Shared.DTOs;

public class NotificationDto
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Data { get; set; }
    public string? ActionUrl { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public record CreateNotificationRequest(
    Guid UserId,
    string Type,
    string Title,
    string Message,
    string? Data = null,
    string? ActionUrl = null
);

public record MarkNotificationAsReadRequest(
    Guid NotificationId
);
