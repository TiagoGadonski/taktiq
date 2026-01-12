namespace GymHero.Domain.Entities;

/// <summary>
/// Represents the result of executing an assessment protocol on a student
/// </summary>
public class AssessmentResult : BaseEntity
{
    // Relations
    public Guid ProtocolId { get; set; }
    public AssessmentProtocol Protocol { get; set; } = null!;

    public Guid StudentId { get; set; }
    public User Student { get; set; } = null!;

    public Guid TrainerId { get; set; }
    public User Trainer { get; set; } = null!;

    // Optional: Link to a StudentAssessment (for grouping related protocols)
    public Guid? StudentAssessmentId { get; set; }
    public StudentAssessment? StudentAssessment { get; set; }

    // Test Data
    public DateTime TestDate { get; set; } = DateTime.UtcNow;

    // Measurements
    // JSON with the actual measurements taken
    // Example: {"distance": 2650, "time": 720, "heartRate": 168}
    public string Measurements { get; set; } = "{}";

    // Calculated Result
    public double? CalculatedScore { get; set; }  // The calculated result (e.g., VO2Max = 52.1)
    public string? ResultUnit { get; set; }       // Unit of the result (e.g., "ml/kg/min", "reps", "seconds")
    public string? Classification { get; set; }   // Based on normative data (e.g., "Excellent", "Good", "Average")

    // Notes
    public string? TrainerNotes { get; set; }     // Private notes from the trainer
    public string? Recommendations { get; set; }  // Auto-generated or manual recommendations

    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
