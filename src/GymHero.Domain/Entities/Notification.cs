namespace GymHero.Domain.Entities;

public class Notification : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public string Type { get; set; } = string.Empty; // FriendRequest, PlanShared, ChallengeCompleted, PlanExpiring, etc.
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    // JSON data for additional context (e.g., friendRequestId, planId, etc.)
    public string? Data { get; set; }

    // Navigation URL (e.g., /friends, /plans/123, etc.)
    public string? ActionUrl { get; set; }

    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }
}
