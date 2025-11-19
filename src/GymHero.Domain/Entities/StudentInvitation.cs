namespace GymHero.Domain.Entities;

public class StudentInvitation : BaseEntity
{
    public Guid TrainerId { get; set; }
    public User Trainer { get; set; } = null!;

    public string StudentEmail { get; set; } = string.Empty;
    public string? StudentName { get; set; }

    public Guid? WorkoutPlanId { get; set; }
    public WorkoutPlan? WorkoutPlan { get; set; }

    public string ActivationToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime? ActivatedAt { get; set; }

    public Guid? CreatedUserId { get; set; } // User ID created after activation
    public User? CreatedUser { get; set; }

    public string Status { get; set; } = "Pending"; // Pending, Activated, Expired
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsExpired => DateTime.UtcNow > ExpiresAt;
    public bool IsActivated => ActivatedAt.HasValue;
}
