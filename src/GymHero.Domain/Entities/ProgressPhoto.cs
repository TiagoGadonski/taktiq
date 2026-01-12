using GymHero.Shared.Enums;

namespace GymHero.Domain.Entities;

/// <summary>
/// Represents a progress photo for tracking student transformation over time
/// </summary>
public class ProgressPhoto : BaseEntity
{
    // Relationships
    public Guid StudentId { get; set; }
    public User Student { get; set; } = null!;

    public Guid MediaId { get; set; }
    public Media Media { get; set; } = null!;

    public Guid UploadedBy { get; set; }  // Could be trainer or student
    public User Uploader { get; set; } = null!;

    // Photo Details
    public ProgressPhotoType PhotoType { get; set; } = ProgressPhotoType.Progress;
    public BodyAngle BodyAngle { get; set; } = BodyAngle.Front;
    public DateTime PhotoDate { get; set; } = DateTime.UtcNow;

    // Optional Measurements (at time of photo)
    public double? WeightKg { get; set; }
    public double? BodyFatPercentage { get; set; }
    public double? MuscleMassKg { get; set; }

    // Circumferences (optional, in cm)
    public double? ChestCm { get; set; }
    public double? WaistCm { get; set; }
    public double? HipsCm { get; set; }
    public double? LeftArmCm { get; set; }
    public double? RightArmCm { get; set; }
    public double? LeftThighCm { get; set; }
    public double? RightThighCm { get; set; }
    public double? LeftCalfCm { get; set; }
    public double? RightCalfCm { get; set; }

    // Notes
    public string? TrainerNotes { get; set; }     // Private notes from trainer
    public string? StudentNotes { get; set; }     // Notes from student
    public string? Caption { get; set; }          // Public caption/description

    // Visibility
    public bool IsVisibleToStudent { get; set; } = true;   // Can student see this photo?
    public bool IsPublic { get; set; } = false;             // Public testimonial photo?

    // Optional link to assessment
    public Guid? StudentAssessmentId { get; set; }
    public StudentAssessment? StudentAssessment { get; set; }

    // Soft delete
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
