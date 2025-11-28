namespace GymHero.Domain.Entities;

/// <summary>
/// Represents a comment on a workout plan
/// </summary>
public class WorkoutPlanComment : BaseEntity
{
    /// <summary>
    /// ID of the workout plan being commented on
    /// </summary>
    public Guid WorkoutPlanId { get; set; }

    /// <summary>
    /// Navigation property to the workout plan
    /// </summary>
    public WorkoutPlan WorkoutPlan { get; set; } = null!;

    /// <summary>
    /// ID of the user who made the comment
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Navigation property to the user
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// The comment text content
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Optional parent comment ID for nested replies
    /// </summary>
    public Guid? ParentCommentId { get; set; }

    /// <summary>
    /// Navigation property to the parent comment
    /// </summary>
    public WorkoutPlanComment? ParentComment { get; set; }

    /// <summary>
    /// Collection of replies to this comment
    /// </summary>
    public ICollection<WorkoutPlanComment> Replies { get; set; } = new List<WorkoutPlanComment>();

    /// <summary>
    /// Whether this comment has been soft-deleted
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// When this comment was marked as deleted
    /// </summary>
    public DateTime? DeletedAt { get; set; }
}
