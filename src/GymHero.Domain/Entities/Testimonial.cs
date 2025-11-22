namespace GymHero.Domain.Entities;

/// <summary>
/// Represents a testimonial/review from a student about their trainer
/// </summary>
public class Testimonial : BaseEntity
{
    /// <summary>
    /// ID of the trainer receiving the testimonial
    /// </summary>
    public Guid TrainerId { get; set; }

    /// <summary>
    /// Navigation property to the trainer
    /// </summary>
    public User Trainer { get; set; } = null!;

    /// <summary>
    /// ID of the student who wrote the testimonial
    /// </summary>
    public Guid StudentId { get; set; }

    /// <summary>
    /// Navigation property to the student
    /// </summary>
    public User Student { get; set; } = null!;

    /// <summary>
    /// The testimonial content/review
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Rating from 1-5 stars
    /// </summary>
    public int Rating { get; set; }

    /// <summary>
    /// When the testimonial was written
    /// </summary>
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Whether the trainer has approved this testimonial for display
    /// </summary>
    public bool IsApproved { get; set; } = false;

    /// <summary>
    /// Optional before/after transformation photos
    /// </summary>
    public string? BeforePhotoUrl { get; set; }
    public string? AfterPhotoUrl { get; set; }

    /// <summary>
    /// Student's transformation results (optional)
    /// </summary>
    public string? TransformationDetails { get; set; }

    /// <summary>
    /// How long the student trained with this trainer
    /// </summary>
    public string? TrainingDuration { get; set; } // e.g., "6 months", "1 year"
}
